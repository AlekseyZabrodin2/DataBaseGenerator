using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataBaseGenerator.Core.DicomGenerator.DicomGeneratorModels;
using DataBaseGenerator.Core.LiteDbData.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels;
using LiteDB;

namespace DataBaseGenerator.Core.DeliveryJournal;

public static class DeliveryJournalStudiesImport
{
    public const string DefaultDicomRoot = @"C:\DicomTempFolder";

    /// <summary>
    /// Creates delivery_records from LiteDB <c>studies</c> (PatientId = <c>SnapshotPatientId</c>).
    /// </summary>
    public static (int Added, int SkippedExisting, int SkippedNoData) AddFromStudiesDatabase(
        string sharedLiteDbPath,
        string deliveryDatabasePath,
        int count,
        PacsDeliveryStatus importStatus,
        string? dicomRootFolder = null,
        IProgress<int>? progress = null)
    {
        if (count <= 0)
            return (0, 0, 0);

        if (string.IsNullOrWhiteSpace(sharedLiteDbPath) || !File.Exists(sharedLiteDbPath))
            throw new FileNotFoundException("LiteDB with studies not found.", sharedLiteDbPath);

        if (string.IsNullOrWhiteSpace(deliveryDatabasePath))
            throw new ArgumentException("Delivery database path is empty.", nameof(deliveryDatabasePath));

        var dicomRoot = string.IsNullOrWhiteSpace(dicomRootFolder) ? DefaultDicomRoot : dicomRootFolder.Trim();
        var now = DateTime.Now;

        using var liteCtx = new LiteDbContext(new ConnectionString(sharedLiteDbPath) { ReadOnly = true });
        using var deliveryRepo = new LiteDbDeliveryRepository(deliveryDatabasePath, readOnly: false);

        var targetKeys = ResolveTargetKeys(deliveryRepo);
        var candidates = BuildCandidates(liteCtx, count);
        var added = 0;
        var skippedExisting = 0;
        var skippedNoData = 0;

        var processed = 0;
        foreach (var candidate in candidates)
        {
            processed++;
            progress?.Report(processed * 100 / candidates.Count);

            if (added >= count)
                break;

            if (deliveryRepo.FindByStudyUid(candidate.StudyInstanceUid) != null)
            {
                skippedExisting++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(candidate.PatientId))
            {
                skippedNoData++;
                continue;
            }

            var files = BuildFiles(candidate, dicomRoot, liteCtx.Images);

            var record = new DeliveryRecord
            {
                StudyInstanceUid = candidate.StudyInstanceUid,
                PatientId = candidate.PatientId,
                TargetKeys = targetKeys,
                Files = files,
                EnqueuedAt = now,
                Status = PacsDeliveryStatus.Pending
            };

            DeliveryRecordState.FillRecordTargets(record, targetKeys);
            DeliveryRecordState.ApplyImportStatus(record, importStatus, now);
            deliveryRepo.Upsert(record);
            added++;
        }

        progress?.Report(100);
        return (added, skippedExisting, skippedNoData);
    }

    private static List<ImportCandidate> BuildCandidates(LiteDbContext ctx, int count) =>
        ctx.Studies.FindAll()
            .OrderByDescending(s => s.StudyDateTime)
            .Take(count)
            .Select(study => new ImportCandidate(
                study.StudyInstanceUid,
                study.SnapshotPatientId?.Trim() ?? string.Empty))
            .ToList();

    private static DeliveryFileRecord[] BuildFiles(
        ImportCandidate candidate,
        string dicomRoot,
        ILiteCollection<LiteImage> images)
    {
        var studyImages = images.Find(i => i.StudyInstanceUid == candidate.StudyInstanceUid).ToList();
        var studyDir = Path.Combine(dicomRoot, candidate.PatientId, candidate.StudyInstanceUid);

        if (Directory.Exists(studyDir))
        {
            var dcmOnDisk = Directory.GetFiles(studyDir, "*.dcm", SearchOption.TopDirectoryOnly);
            if (dcmOnDisk.Length > 0)
            {
                return dcmOnDisk.Select((path, index) =>
                {
                    var sop = studyImages.ElementAtOrDefault(index)?.SopInstanceUid
                              ?? Path.GetFileNameWithoutExtension(path);
                    return new DeliveryFileRecord
                    {
                        FilePath = path,
                        SopInstanceUid = UidGenerator.NormalizeUid(sop)
                    };
                }).ToArray();
            }
        }

        if (studyImages.Count > 0)
        {
            return studyImages.Select(image => new DeliveryFileRecord
            {
                FilePath = Path.Combine(studyDir, image.SopInstanceUid + ".dcm"),
                SopInstanceUid = UidGenerator.NormalizeUid(image.SopInstanceUid)
            }).ToArray();
        }

        var syntheticSop = UidGenerator.GenerateImageUid();
        return
        [
            new DeliveryFileRecord
            {
                FilePath = Path.Combine(studyDir, $"DX-P-1-{syntheticSop}.dcm"),
                SopInstanceUid = syntheticSop
            }
        ];
    }

    private static string[] ResolveTargetKeys(LiteDbDeliveryRepository deliveryRepo)
    {
        var sample = deliveryRepo.GetPage(statusFilter: null, patientIdPrefix: null, skip: 0).FirstOrDefault();
        if (sample?.TargetKeys is { Length: > 0 })
            return sample.TargetKeys;

        return
        [
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            "Local|LocalScp|LocalScp"
        ];
    }

    private sealed record ImportCandidate(string StudyInstanceUid, string PatientId);
}
