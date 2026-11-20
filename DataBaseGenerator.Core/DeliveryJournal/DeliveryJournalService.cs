using System;
using System.Collections.Generic;
using System.Linq;

namespace DataBaseGenerator.Core.DeliveryJournal;

public sealed class DeliveryJournalRow
{
    public int RowNumber { get; init; }

    public string StudyInstanceUid { get; init; } = string.Empty;
    public string PatientId { get; init; } = string.Empty;
    public PacsDeliveryStatus Status { get; init; }
    public PacsDeliveryFailureKind FailureKind { get; init; }
    public string? ErrorMessage { get; init; }
    public int FileCount { get; init; }
    public DateTime EnqueuedAt { get; init; }
    public DateTime? LastAttemptAt { get; init; }
}

public static class DeliveryJournalService
{
    public static IReadOnlyList<DeliveryJournalRow> LoadRows(
        string databasePath,
        PacsDeliveryStatus? statusFilter)
    {
        using var repo = new LiteDbDeliveryRepository(databasePath, readOnly: true);
        var rows = repo.LoadSummaries();
        if (statusFilter is null)
            return rows;

        var filtered = new List<DeliveryJournalRow>(rows.Count);
        foreach (var row in rows)
        {
            if (row.Status != statusFilter.Value)
                continue;

            filtered.Add(new DeliveryJournalRow
            {
                RowNumber = filtered.Count + 1,
                StudyInstanceUid = row.StudyInstanceUid,
                PatientId = row.PatientId,
                Status = row.Status,
                FailureKind = row.FailureKind,
                ErrorMessage = row.ErrorMessage,
                FileCount = row.FileCount,
                EnqueuedAt = row.EnqueuedAt,
                LastAttemptAt = row.LastAttemptAt
            });
        }

        return filtered;
    }

    public static DeliveryRecord? LoadRecord(string databasePath, string studyUid)
    {
        using var repo = new LiteDbDeliveryRepository(databasePath, readOnly: true);
        return repo.FindByStudyUid(studyUid);
    }

    /// <summary>
    /// Changes up to <paramref name="count"/> records matching <paramref name="sourceStatusFilter"/>
    /// (any status when filter is null) to <paramref name="targetStatus"/>.
    /// </summary>
    public static (int Updated, int Available) ApplyStatusToRecords(
        string databasePath,
        PacsDeliveryStatus targetStatus,
        int count,
        PacsDeliveryStatus? sourceStatusFilter)
    {
        if (count <= 0)
            return (0, 0);

        using var repo = new LiteDbDeliveryRepository(databasePath, readOnly: false);

        IReadOnlyList<DeliveryRecord> pool = sourceStatusFilter is null
            ? repo.GetPage(statusFilter: null, patientIdPrefix: null, skip: 0)
            : repo.GetByStatus(sourceStatusFilter.Value);

        var eligible = pool.Where(r => r.Status != targetStatus).ToList();
        var available = eligible.Count;
        var candidates = eligible.Take(count).ToList();
        var now = DateTime.Now;
        var updated = 0;

        foreach (var record in candidates)
        {
            DeliveryRecordState.ApplyImportStatus(record, targetStatus, now);
            repo.Upsert(record);
            updated++;
        }

        return (updated, available);
    }

    /// <summary>
    /// Applies <paramref name="targetStatus"/> to up to <paramref name="count"/> records
    /// in the order of <paramref name="studyUidsInDisplayOrder"/> (current table selection).
    /// </summary>
    public static (int Updated, int Available) ApplyStatusToRecordsByStudyUids(
        string databasePath,
        PacsDeliveryStatus targetStatus,
        int count,
        IReadOnlyList<string> studyUidsInDisplayOrder,
        IProgress<int>? progress = null)
    {
        if (count <= 0 || studyUidsInDisplayOrder.Count == 0)
            return (0, 0);

        using var repo = new LiteDbDeliveryRepository(databasePath, readOnly: false);
        var pool = new List<DeliveryRecord>(studyUidsInDisplayOrder.Count);

        foreach (var uid in studyUidsInDisplayOrder)
        {
            var record = repo.FindByStudyUid(uid);
            if (record is not null && record.Status != targetStatus)
                pool.Add(record);
        }

        var available = pool.Count;
        var candidates = pool.Take(count).ToList();
        var now = DateTime.Now;
        var updated = 0;
        var processed = 0;

        foreach (var record in candidates)
        {
            DeliveryRecordState.ApplyImportStatus(record, targetStatus, now);
            repo.Upsert(record);
            updated++;
            processed++;
            progress?.Report(processed * 100 / candidates.Count);
        }

        progress?.Report(100);
        return (updated, available);
    }

    public static int ClearAllRecords(string databasePath)
    {
        using var repo = new LiteDbDeliveryRepository(databasePath, readOnly: false);
        return repo.DeleteAll();
    }

    public static (int Added, int SkippedExisting, int SkippedNoData) AddFromStudiesDatabase(
        string sharedLiteDbPath,
        string deliveryDatabasePath,
        int count,
        PacsDeliveryStatus importStatus,
        string? dicomRootFolder = null,
        IProgress<int>? progress = null) =>
        DeliveryJournalStudiesImport.AddFromStudiesDatabase(
            sharedLiteDbPath,
            deliveryDatabasePath,
            count,
            importStatus,
            dicomRootFolder,
            progress);
}
