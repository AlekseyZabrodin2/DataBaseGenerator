using System;
using LiteDB;

namespace DataBaseGenerator.Core.DeliveryJournal;

public sealed class DeliveryRecord
{
    [BsonId]
    public string StudyInstanceUid { get; set; } = default!;

    public string PatientId { get; set; } = string.Empty;
    public PacsDeliveryStatus Status { get; set; }
    public PacsDeliveryFailureKind FailureKind { get; set; }
    public string[]? TargetKeys { get; set; }
    public DeliveryFileRecord[] Files { get; set; } = [];
    public DateTime EnqueuedAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class DeliveryFileRecord
{
    public string FilePath { get; set; } = string.Empty;
    public string SopInstanceUid { get; set; } = string.Empty;
    public TargetDeliveryRecord[] TargetDeliveries { get; set; } = [];
}

public sealed class TargetDeliveryRecord
{
    public string TargetKey { get; set; } = string.Empty;
    public PacsDeliveryStatus Status { get; set; }
    public PacsDeliveryFailureKind FailureKind { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}
