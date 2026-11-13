using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace DataBaseGenerator.Core.DeliveryJournal;

public sealed class LiteDbDeliveryRepository : IDisposable
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<DeliveryRecord> _collection;

    public LiteDbDeliveryRepository(string databasePath)
    {
        _database = new LiteDatabase(new ConnectionString(databasePath) { ReadOnly = false });
        _collection = _database.GetCollection<DeliveryRecord>("delivery_records");
    }

    public LiteDbDeliveryRepository(string databasePath, bool readOnly)
    {
        _database = new LiteDatabase(new ConnectionString(databasePath) { ReadOnly = readOnly });
        _collection = _database.GetCollection<DeliveryRecord>("delivery_records");
    }

    public void Upsert(DeliveryRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        record.StudyInstanceUid = record.StudyInstanceUid.Trim();
        _collection.Upsert(Normalize(record));
    }

    public DeliveryRecord? FindByStudyUid(string studyUid)
    {
        if (string.IsNullOrWhiteSpace(studyUid))
            return null;

        var record = _collection.FindById(studyUid.Trim());
        return record is null ? null : Normalize(record);
    }

    public IReadOnlyList<DeliveryRecord> GetByStatus(PacsDeliveryStatus status) =>
        _collection.Find(r => r.Status == status)
            .OrderByDescending(r => r.EnqueuedAt)
            .Select(Normalize)
            .ToList();

    public int CountByStatus(PacsDeliveryStatus status) =>
        _collection.Count(r => r.Status == status);

    public IReadOnlyList<DeliveryRecord> GetPage(
        PacsDeliveryStatus? statusFilter,
        string? patientIdPrefix,
        int skip)
    {
        var source = statusFilter is null
            ? _collection.FindAll().AsEnumerable()
            : _collection.Find(r => r.Status == statusFilter);

        if (!string.IsNullOrWhiteSpace(patientIdPrefix))
        {
            var prefix = patientIdPrefix.Trim();
            source = source.Where(r =>
                !string.IsNullOrEmpty(r.PatientId)
                && r.PatientId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        return source
            .OrderByDescending(r => r.EnqueuedAt)
            .Skip(skip)
            .Select(Normalize)
            .ToList();
    }

    /// <summary>
    /// Grid rows only. Skips materializing Files and target deliveries; the grid needs the file count.
    /// </summary>
    public IReadOnlyList<DeliveryJournalRow> LoadSummaries()
    {
        var docs = _database.GetCollection("delivery_records").FindAll().ToList();
        docs.Sort(static (a, b) => ReadDate(b, "EnqueuedAt").CompareTo(ReadDate(a, "EnqueuedAt")));

        var rows = new List<DeliveryJournalRow>(docs.Count);
        for (var i = 0; i < docs.Count; i++)
            rows.Add(ToSummary(docs[i], i + 1));

        return rows;
    }

    public int Count(PacsDeliveryStatus? statusFilter, string? patientIdPrefix = null)
    {
        if (statusFilter is null && string.IsNullOrWhiteSpace(patientIdPrefix))
            return _collection.Count();

        return GetPage(statusFilter, patientIdPrefix, skip: 0).Count;
    }

    public int DeleteAll()
    {
        var count = _collection.Count();
        _collection.DeleteAll();
        return count;
    }

    public void Dispose() => _database.Dispose();

    private static DeliveryJournalRow ToSummary(BsonDocument doc, int rowNumber)
    {
        var files = doc.TryGetValue("Files", out var filesValue) && filesValue.IsArray
            ? filesValue.AsArray.Count
            : 0;

        return new DeliveryJournalRow
        {
            RowNumber = rowNumber,
            StudyInstanceUid = ReadId(doc),
            PatientId = ReadString(doc, "PatientId") ?? string.Empty,
            Status = ReadEnum<PacsDeliveryStatus>(doc, "Status"),
            FailureKind = ReadEnum<PacsDeliveryFailureKind>(doc, "FailureKind"),
            ErrorMessage = ReadString(doc, "ErrorMessage"),
            FileCount = files,
            EnqueuedAt = ReadDate(doc, "EnqueuedAt"),
            LastAttemptAt = ReadDateOrNull(doc, "LastAttemptAt")
        };
    }

    private static string ReadId(BsonDocument doc)
    {
        if (doc.TryGetValue("_id", out var id) && id.IsString)
            return id.AsString;
        return ReadString(doc, "StudyInstanceUid") ?? string.Empty;
    }

    private static string? ReadString(BsonDocument doc, string name)
    {
        if (!doc.TryGetValue(name, out var value) || value.IsNull)
            return null;
        return value.IsString ? value.AsString : value.ToString();
    }

    private static DateTime ReadDate(BsonDocument doc, string name) =>
        ReadDateOrNull(doc, name) ?? default;

    private static DateTime? ReadDateOrNull(BsonDocument doc, string name)
    {
        if (!doc.TryGetValue(name, out var value) || value.IsNull || !value.IsDateTime)
            return null;
        return value.AsDateTime;
    }

    private static TEnum ReadEnum<TEnum>(BsonDocument doc, string name)
        where TEnum : struct, Enum
    {
        if (!doc.TryGetValue(name, out var value) || value.IsNull)
            return default;

        if (value.IsString && Enum.TryParse<TEnum>(value.AsString, ignoreCase: true, out var parsed))
            return parsed;

        if (value.IsNumber)
        {
            var number = value.IsInt32 ? value.AsInt32 : value.IsInt64 ? value.AsInt64 : (long)value.AsDouble;
            return (TEnum)Enum.ToObject(typeof(TEnum), number);
        }

        return default;
    }

    private static DeliveryRecord Normalize(DeliveryRecord record)
    {
        record.Files ??= [];
        foreach (var file in record.Files)
            file.TargetDeliveries ??= [];
        return record;
    }
}
