using System;
using System.Collections.Generic;
using System.Linq;

namespace DataBaseGenerator.Core.DeliveryJournal;

public static class DeliveryRecordState
{
    public static void FillRecordTargets(DeliveryRecord record, IEnumerable<string> configuredTargets)
    {
        var preserveFailure = record.Status == PacsDeliveryStatus.Failed
            && record.Files.All(file => (file.TargetDeliveries?.Length ?? 0) == 0);

        record.TargetKeys ??= configuredTargets.Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        foreach (var file in record.Files)
        {
            var deliveries = (file.TargetDeliveries ?? []).ToList();
            foreach (var target in record.TargetKeys)
            {
                if (deliveries.Any(delivery => SameTarget(delivery.TargetKey, target)))
                    continue;

                deliveries.Add(new TargetDeliveryRecord
                {
                    TargetKey = target,
                    Status = preserveFailure ? PacsDeliveryStatus.Failed : PacsDeliveryStatus.Pending,
                    FailureKind = preserveFailure ? record.FailureKind : PacsDeliveryFailureKind.None,
                    ErrorMessage = preserveFailure ? record.ErrorMessage : null,
                    LastAttemptAt = preserveFailure ? record.LastAttemptAt : null
                });
            }

            file.TargetDeliveries = deliveries.ToArray();
        }
    }

    public static void UpdateSummaryFromFiles(DeliveryRecord record)
    {
        if (record.Status == PacsDeliveryStatus.NotRequired)
            return;

        var targetKeys = record.TargetKeys ?? [];
        var expectedDeliveries = (from file in record.Files
            from target in targetKeys
            select file.TargetDeliveries.FirstOrDefault(delivery => SameTarget(delivery.TargetKey, target))).ToArray();

        var isComplete = record.TargetKeys is { Length: 0 }
            || (record.Files.Length > 0 && targetKeys.Length > 0
                && record.Files.All(file => !string.IsNullOrWhiteSpace(file.FilePath) && !string.IsNullOrWhiteSpace(file.SopInstanceUid))
                && expectedDeliveries.All(delivery => delivery != null));

        var failedDelivery = expectedDeliveries.FirstOrDefault(delivery => delivery?.Status == PacsDeliveryStatus.Failed);

        if (expectedDeliveries.Any(delivery => delivery?.Status == PacsDeliveryStatus.Sending))
        {
            record.Status = PacsDeliveryStatus.Sending;
        }
        else if (isComplete && expectedDeliveries.All(delivery => delivery!.Status == PacsDeliveryStatus.Delivered))
        {
            record.Status = PacsDeliveryStatus.Delivered;
        }
        else
        {
            record.Status = failedDelivery != null || !isComplete ? PacsDeliveryStatus.Failed : PacsDeliveryStatus.Pending;
        }

        record.FailureKind = failedDelivery?.FailureKind ?? PacsDeliveryFailureKind.None;
        record.ErrorMessage = record.Status == PacsDeliveryStatus.Failed
            ? failedDelivery?.ErrorMessage ?? "Delivery manifest or recipient set is incomplete."
            : null;

        record.LastAttemptAt = expectedDeliveries.Where(delivery => delivery != null)
            .Select(delivery => delivery!.LastAttemptAt).DefaultIfEmpty().Max();
    }

    public static void SetAllTargetsDelivered(DeliveryRecord record, DateTime attemptAt)
    {
        record.Status = PacsDeliveryStatus.Delivered;
        record.FailureKind = PacsDeliveryFailureKind.None;
        record.ErrorMessage = null;
        record.LastAttemptAt = attemptAt;

        if (record.TargetKeys is { Length: > 0 })
            FillRecordTargets(record, record.TargetKeys);

        foreach (var file in record.Files)
        {
            file.TargetDeliveries ??= [];

            foreach (var delivery in file.TargetDeliveries)
            {
                delivery.Status = PacsDeliveryStatus.Delivered;
                delivery.FailureKind = PacsDeliveryFailureKind.None;
                delivery.ErrorMessage = null;
                delivery.LastAttemptAt = attemptAt;
            }
        }

        record.Status = PacsDeliveryStatus.Delivered;
    }

    public static void ApplyImportStatus(DeliveryRecord record, PacsDeliveryStatus status, DateTime attemptAt)
    {
        switch (status)
        {
            case PacsDeliveryStatus.Delivered:
                SetAllTargetsDelivered(record, attemptAt);
                return;
            case PacsDeliveryStatus.Failed:
                MarkAllTargetsFailed(
                    record,
                    PacsDeliveryFailureKind.ConnectionUnavailable,
                    "Marked failed for test (DataBase Generator)",
                    attemptAt);
                return;
            case PacsDeliveryStatus.NotRequired:
                record.Status = PacsDeliveryStatus.NotRequired;
                record.FailureKind = PacsDeliveryFailureKind.None;
                record.ErrorMessage = null;
                record.LastAttemptAt = null;
                if (record.TargetKeys is { Length: > 0 })
                    FillRecordTargets(record, record.TargetKeys);
                return;
            case PacsDeliveryStatus.Sending:
                SetAllTargetsStatus(record, PacsDeliveryStatus.Sending, attemptAt, setLastAttempt: true);
                return;
            case PacsDeliveryStatus.Pending:
            default:
                SetAllTargetsStatus(record, PacsDeliveryStatus.Pending, attemptAt, setLastAttempt: false);
                return;
        }
    }

    private static void SetAllTargetsStatus(
        DeliveryRecord record,
        PacsDeliveryStatus status,
        DateTime attemptAt,
        bool setLastAttempt)
    {
        record.FailureKind = PacsDeliveryFailureKind.None;
        record.ErrorMessage = null;
        record.LastAttemptAt = setLastAttempt ? attemptAt : null;

        if (record.TargetKeys is { Length: > 0 })
            FillRecordTargets(record, record.TargetKeys);

        foreach (var file in record.Files)
        {
            file.TargetDeliveries ??= [];

            foreach (var delivery in file.TargetDeliveries)
            {
                delivery.Status = status;
                delivery.FailureKind = PacsDeliveryFailureKind.None;
                delivery.ErrorMessage = null;
                delivery.LastAttemptAt = setLastAttempt ? attemptAt : null;
            }
        }

        record.Status = status;
    }

    public static void MarkAllTargetsFailed(
        DeliveryRecord record,
        PacsDeliveryFailureKind failureKind,
        string errorMessage,
        DateTime attemptAt)
    {
        record.Status = PacsDeliveryStatus.Failed;
        record.FailureKind = failureKind;
        record.ErrorMessage = errorMessage;
        record.LastAttemptAt = attemptAt;

        if (record.TargetKeys is { Length: > 0 })
            FillRecordTargets(record, record.TargetKeys);

        foreach (var file in record.Files)
        {
            file.TargetDeliveries ??= [];

            foreach (var delivery in file.TargetDeliveries)
            {
                delivery.Status = PacsDeliveryStatus.Failed;
                delivery.FailureKind = failureKind;
                delivery.ErrorMessage = errorMessage;
                delivery.LastAttemptAt = attemptAt;
            }
        }
    }

    private static bool SameTarget(string left, string right) =>
        Guid.TryParse(left, out var leftId) && Guid.TryParse(right, out var rightId)
            ? leftId == rightId
            : string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
