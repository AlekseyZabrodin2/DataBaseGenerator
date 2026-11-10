using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DataBaseGenerator.Core.DicomGenerator.DicomGeneratorModels;
using DataBaseGenerator.Core.GeneratorRules.PlannedStudy;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using FellowOakDicom;

namespace DataBaseGenerator.Core.LiteDbGenerator.PlannedStudy
{
  public sealed class PlannedStudyDicomStubWriter
  {
    public const string DefaultTempFolder = @"C:\DicomTempFolder";

    private const string DxPresentationSopClass = "1.2.840.10008.5.1.4.1.1.1.1";
    private const string ManufacturerUidRoot = "1.2.826.0.1.3680043.2.564";

    // UniExpert planned DX stub (same private / nested tags as reference dump)
    private static readonly DicomTag AdaniModalityPrivateTag = new(0x0031, 0x0401);
    private static readonly DicomTag UniExpertViewCodeSequenceTag = new(0x0054, 0x0220);
    private static readonly DicomTag UniExpertViewModifierCodeSequenceTag = new(0x0054, 0x0222);

    public (PlannedStudyArea[] Areas, List<(string Description, string LateralityCode)> Details) CreateStudyAreas(
      RandomBodyPartDataRule bodyPartRule,
      Random random)
    {
      var areas = new List<PlannedStudyArea>();
      var details = new List<(string Description, string LateralityCode)>();
      var count = random.Next(1, 4);

      for (var i = 0; i < count; i++)
      {
        var data = bodyPartRule.Generate();
        areas.Add(new PlannedStudyArea
        {
          BodyPartGuid = data.bodyPartGuid,
          AnatomicRegionCodeValue = data.anatomicCode,
          ViewCodeValue = data.viewCode,
          ProjectionUid = PlannedStudyCatalogProjectionUids.Resolve(
              data.bodyPartGuid,
              data.anatomicCode,
              data.viewCode),
          LateralityCode = data.lateralityCode
        }.Normalize());
        details.Add((data.description, data.lateralityCode));
      }

      return (areas.ToArray(), details);
    }

    public string[] WriteStubs(
      string rootFolder,
      PlannedStudyLiteDb study,
      PlannedStudyArea[] areas,
      IReadOnlyList<(string Description, string LateralityCode)> areaDetails)
    {
      if (study == null)
        throw new ArgumentNullException(nameof(study));
      if (areas == null || areas.Length == 0)
        return Array.Empty<string>();

      var patientId = study.PatientId ?? string.Empty;
      var studyUid = study.StudyInstanceUid ?? study.Id;
      if (string.IsNullOrWhiteSpace(patientId) || string.IsNullOrWhiteSpace(studyUid))
        throw new InvalidOperationException("PatientId and StudyInstanceUid are required to write planned DICOM stubs.");

      var studyDirectory = Path.Combine(rootFolder, patientId, studyUid);
      Directory.CreateDirectory(studyDirectory);

      var studyDateTime = study.StudyDateTime ?? DateTime.Now;
      var paths = new string[areas.Length];
      var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

      for (var index = 0; index < areas.Length; index++)
      {
        var area = areas[index];
        var detail = index < areaDetails.Count ? areaDetails[index] : default;
        var frameOfReferenceUid = UidGenerator.NormalizeUid(UidGenerator.GenerateTargetUid());
        var sopInstanceUid = UidGenerator.NormalizeUid(GenerateAdaniSopInstanceUid());
        var seriesInstanceUid = UidGenerator.NormalizeUid(UidGenerator.GenerateSeriesUid());

        var letter = letters[Random.Shared.Next(letters.Length)];
        var fileName = $"DX-{letter}-{index + 1}-{frameOfReferenceUid}.dcm";
        var filePath = Path.Combine(studyDirectory, fileName);

        var dataset = BuildDataset(
          study,
          area,
          detail.Description,
          studyDateTime,
          sopInstanceUid,
          seriesInstanceUid,
          frameOfReferenceUid,
          index + 1);
        var file = new DicomFile(dataset);
        file.Save(filePath);
        paths[index] = filePath;
      }

      return paths;
    }

    private static DicomDataset BuildDataset(
      PlannedStudyLiteDb study,
      PlannedStudyArea area,
      string bodyPartDescription,
      DateTime studyDateTime,
      string sopInstanceUid,
      string seriesInstanceUid,
      string frameOfReferenceUid,
      int instanceNumber)
    {
      var instanceCreated = PickInstanceCreationDateTime(studyDateTime);
      var dataset = new DicomDataset
      {
        { DicomTag.SpecificCharacterSet, "ISO_IR 192" },
        { DicomTag.ImageType, "ORIGINAL", "PRIMARY" },
        { DicomTag.SOPClassUID, DxPresentationSopClass },
        { DicomTag.SOPInstanceUID, UidGenerator.NormalizeUid(sopInstanceUid) },
        { DicomTag.StudyDate, studyDateTime.Date },
        { DicomTag.StudyTime, new DateTime(studyDateTime.TimeOfDay.Ticks) },
        { DicomTag.InstanceCreationDate, instanceCreated.Date },
        { DicomTag.InstanceCreationTime, new DateTime(instanceCreated.TimeOfDay.Ticks) },
        { DicomTag.Modality, "DX" },
        { DicomTag.Manufacturer, "ADVIN SOLUTIONS" },
        { DicomTag.StationName, "Uniexpert 001" },
        { DicomTag.OperatorsName, "Service Adani" },
        { DicomTag.ManufacturerModelName, "UniExpert" },
        { DicomTag.PatientName, FormatPatientName(study) },
        { DicomTag.PatientID, study.PatientId ?? string.Empty },
        { DicomTag.AccessionNumber, string.Empty },
        { DicomTag.StudyInstanceUID, UidGenerator.NormalizeUid(study.StudyInstanceUid ?? study.Id) },
        { DicomTag.SeriesInstanceUID, UidGenerator.NormalizeUid(seriesInstanceUid) },
        { DicomTag.StudyID, TrimSh(study.StudyID) },
        { DicomTag.SeriesNumber, instanceNumber.ToString(CultureInfo.InvariantCulture) },
        { DicomTag.InstanceNumber, instanceNumber.ToString(CultureInfo.InvariantCulture) },
        { DicomTag.FrameOfReferenceUID, UidGenerator.NormalizeUid(frameOfReferenceUid) },
        { DicomTag.ImageLaterality, MapLaterality(area.LateralityCode) },
        { DicomTag.PerformedProcedureStepStatus, "NOT STARTED" },
        { DicomTag.BodyPartExamined, InferBodyPartExamined(bodyPartDescription) },
        { DicomTag.DeviceSerialNumber, "Uniexpert 001" },
        { DicomTag.SoftwareVersions, "UniExpert" },
        { DicomTag.PositionerType, "CARM" },
        { DicomTag.PatientOrientation, "L", "A" }
      };
      dataset.AutoValidate = false;

      if (study.BirthDate.HasValue)
        dataset.AddOrUpdate(DicomTag.PatientBirthDate, study.BirthDate.Value.Date);

      dataset.AddOrUpdate(DicomTag.PatientSex, MapSex(study.Gender));

      if (!string.IsNullOrWhiteSpace(study.Age))
        dataset.AddOrUpdate(DicomTag.PatientAge, study.Age);

      if (!string.IsNullOrWhiteSpace(study.Address))
        dataset.AddOrUpdate(DicomTag.PatientAddress, study.Address);

      if (!string.IsNullOrWhiteSpace(study.Telephone))
        dataset.AddOrUpdate(DicomTag.PatientTelephoneNumbers, study.Telephone);

      if (!string.IsNullOrWhiteSpace(study.Comments))
        dataset.AddOrUpdate(DicomTag.PatientComments, study.Comments);

      if (!string.IsNullOrWhiteSpace(area.AnatomicRegionCodeValue))
      {
        var regionItem = new DicomDataset
        {
          { DicomTag.CodeValue, area.AnatomicRegionCodeValue },
          { DicomTag.CodingSchemeDesignator, "SCT" },
          { DicomTag.CodeMeaning, InferRegionMeaning(bodyPartDescription) }
        };
        dataset.AddOrUpdate(DicomTag.AnatomicRegionSequence, regionItem);
      }

      // (0040,0555) Acquisition Context Sequence — one placeholder item like UniExpert stub
      var acquisitionContextItem = new DicomDataset();
      dataset.AddOrUpdate(DicomTag.AcquisitionContextSequence, acquisitionContextItem);

      if (!string.IsNullOrWhiteSpace(area.ViewCodeValue))
      {
        var viewItem = new DicomDataset
        {
          { DicomTag.CodeValue, area.ViewCodeValue },
          { DicomTag.CodingSchemeDesignator, "SCT" },
          { DicomTag.CodeMeaning, InferViewMeaning(bodyPartDescription) }
        };

        if (!string.IsNullOrWhiteSpace(area.ProjectionUid))
          viewItem.AddOrUpdate(DicomTag.ContextUID, UidGenerator.NormalizeUid(area.ProjectionUid));

        var viewItemEtalon = viewItem.Clone();
        dataset.AddOrUpdate(DicomTag.ViewCodeSequence, viewItem);
        dataset.AddOrUpdate(UniExpertViewCodeSequenceTag, viewItemEtalon);

        var viewModifierItem = new DicomDataset
        {
          { DicomTag.CodeValue, area.ViewCodeValue },
          { DicomTag.CodingSchemeDesignator, "SCT" },
          { DicomTag.CodeMeaning, InferViewMeaning(bodyPartDescription) }
        };
        dataset.AddOrUpdate(UniExpertViewModifierCodeSequenceTag, viewModifierItem);
      }

      dataset.Add(DicomVR.CS, AdaniModalityPrivateTag, "DX");
      dataset.AddOrUpdate(DicomTag.RequestedProcedureDescription, string.Empty);

      return dataset;
    }

    /// <summary>
    /// UniExpert expects StudyAreas and ProjectionPaths to match 1:1; drop entries without files.
    /// </summary>
    public static void AlignArtifacts(PlannedStudyLiteDb study, PlannedStudyArea[] areas, string[] paths)
    {
      if (study == null)
        return;

      if (paths == null || paths.Length == 0 || areas == null || areas.Length == 0)
      {
        study.StudyAreas = Array.Empty<PlannedStudyArea>();
        study.ProjectionPaths = Array.Empty<string>();
        return;
      }

      var paired = new List<(PlannedStudyArea Area, string Path)>();
      var count = Math.Min(areas.Length, paths.Length);

      for (var i = 0; i < count; i++)
      {
        var path = paths[i];
        var area = areas[i];
        if (area == null || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
          continue;

        paired.Add((area, path));
      }

      study.StudyAreas = paired.Select(p => p.Area).ToArray();
      study.ProjectionPaths = paired.Select(p => p.Path).ToArray();
    }

    private static string FormatPatientName(PlannedStudyLiteDb study)
    {
      var parts = new[] { study.PatientLastName, study.PatientFirstName, study.PatientMiddleName }
        .Where(p => !string.IsNullOrWhiteSpace(p));
      return string.Join("^", parts);
    }

    private static string MapSex(PatientSex sex) =>
      sex switch
      {
        PatientSex.Male => "M",
        PatientSex.Female => "F",
        _ => "O"
      };

    private static string MapLaterality(string lateralityCode)
    {
      if (string.IsNullOrWhiteSpace(lateralityCode))
        return "U";

      return lateralityCode.ToUpperInvariant() switch
      {
        "L" => "L",
        "R" => "R",
        "B" => "B",
        "P" => "P",
        _ => "U"
      };
    }

    private static string InferBodyPartExamined(string description)
    {
      if (string.IsNullOrWhiteSpace(description))
        return "UNKNOWN";

      var segment = description.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
      return string.IsNullOrWhiteSpace(segment)
        ? "UNKNOWN"
        : segment.ToUpperInvariant().Replace(' ', '_');
    }

    private static string InferRegionMeaning(string description)
    {
      if (string.IsNullOrWhiteSpace(description))
        return "Planned region";

      var segment = description.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault();
      return string.IsNullOrWhiteSpace(segment) ? "Planned region" : segment;
    }

    private static string InferViewMeaning(string description)
    {
      if (string.IsNullOrWhiteSpace(description) || !description.Contains('('))
        return "Planned view";

      var start = description.LastIndexOf('(');
      var end = description.LastIndexOf(')');
      if (start < 0 || end <= start)
        return "Planned view";

      return description.Substring(start + 1, end - start - 1);
    }

    /// <summary>Instance creation not before planned study date/time; not after stub write moment.</summary>
    private static DateTime PickInstanceCreationDateTime(DateTime studyDateTime)
    {
      var end = DateTime.Now;
      var start = studyDateTime;

      if (start > end)
        return end;

      var spanTicks = end.Ticks - start.Ticks;
      if (spanTicks <= 0)
        return end;

      return start.AddTicks(Random.Shared.NextInt64(0, spanTicks + 1));
    }

    private static string GenerateAdaniSopInstanceUid()
    {
      // Pattern like 1.2.826.0.1.3680043.2.564.2017612.102847.121.639299128787235027
      var uid = $"{ManufacturerUidRoot}.{Random.Shared.Next(1_000_000, 9_999_999)}." +
                $"{Random.Shared.Next(100_000, 999_999)}.{Random.Shared.Next(100, 999)}." +
                $"{Random.Shared.NextInt64(100_000_000_000L, 999_999_999_999L)}";
      return UidGenerator.NormalizeUid(uid);
    }

    private static string TrimSh(string value)
    {
      if (string.IsNullOrEmpty(value))
        return string.Empty;

      return value.Length <= 16 ? value : value[..16];
    }
  }
}
