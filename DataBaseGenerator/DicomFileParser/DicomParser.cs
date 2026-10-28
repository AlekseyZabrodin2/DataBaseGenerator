using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using FellowOakDicom;
using LiteDB;

namespace DataBaseGenerator.UI.Wpf.DicomFileParser
{
    public class DicomParser
    {
        public DicomFileInfo ParseFile(string filePath)
        {
            try
            {
                return ParseDataset(DicomFile.Open(filePath).Dataset);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing {filePath}: {ex.Message}");
                return null;
            }
        }

        public DicomFileInfo ParseDataset(DicomDataset dataset)
        {
            try
            {
                var name = ParseDicomName(dataset.GetStringValue(DicomTag.PatientName, string.Empty));
                var birthDate = dataset.GetDateTimeValue(DicomTag.PatientBirthDate);

                var patient = new PatientLiteDb()
                {
                    Id = null,
                    PatientID = dataset.GetStringValue(DicomTag.PatientID),
                    LastName = name.lastName,
                    FirstName = name.firstName,
                    MiddleName = name.middleName,
                    BirthDate = birthDate,
                    Sex = ParsePatientSex(dataset.GetStringValue(DicomTag.PatientSex, string.Empty)),
                    Phone = dataset.GetStringValue(DicomTag.PatientTelephoneNumbers, string.Empty),
                    Address = dataset.GetStringValue(DicomTag.PatientAddress, string.Empty),
                    Comments = dataset.GetStringValue(DicomTag.PatientComments, string.Empty),
                    Occupation = dataset.GetStringValue(DicomTag.Occupation, string.Empty),
                    Age = CalculateAge(birthDate)
                };

                patient.FullName = $"{patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim();

                var study = new StudyLiteDb()
                {
                    Id = ObjectId.NewObjectId(),
                    StudyInstanceUid = dataset.GetStringValue(DicomTag.StudyInstanceUID, string.Empty),
                    StudyId = dataset.GetStringValue(DicomTag.StudyID, string.Empty),
                    SnapshotLastName = patient.LastName,
                    SnapshotFirstName = patient.FirstName,
                    SnapshotPatronymic = patient.MiddleName,
                    SnapshotPatientId = patient.PatientID,
                    SnapshotPatientSex = patient.Sex,
                    PatientBirthDate = patient.BirthDate,
                    AccessionNumber = dataset.GetStringValue(DicomTag.AccessionNumber, string.Empty),
                    BodyParts = GetBodyParts(dataset),
                    StudyDateTime = GetStudyDateTime(dataset) ?? default,
                    Status = string.Empty
                };

                var series = new SeriesLiteDb()
                {
                    Id = ObjectId.NewObjectId(),
                    SeriesInstanceUid = dataset.GetStringValue(DicomTag.SeriesInstanceUID, string.Empty),
                    StudyInstanceUid = study.StudyInstanceUid,
                    OperatorName = dataset.GetStringValue(DicomTag.OperatorsName, string.Empty),
                    Modality = dataset.GetStringValue(DicomTag.Modality, string.Empty),
                    BodyPartExamined = dataset.GetStringValue(DicomTag.BodyPartExamined, string.Empty),
                    SeriesDateTime = GetSeriesDateTime(dataset) ?? study.StudyDateTime
                };

                var image = new ImageLiteDb()
                {
                    Id = ObjectId.NewObjectId(),
                    SopInstanceUid = dataset.GetStringValue(DicomTag.SOPInstanceUID, string.Empty),
                    SeriesInstanceUid = series.SeriesInstanceUid,
                    StudyInstanceUid = study.StudyInstanceUid,
                    BodyPart = dataset.GetStringValue(DicomTag.BodyPartExamined, string.Empty),
                    Projection = GetProjection(dataset),
                    Laterality = GetLaterality(dataset),
                    InstanceDose = GetDose(dataset),
                    AcquisitionTime = GetAcquisitionTime(dataset) ?? series.SeriesDateTime
                };

                return new DicomFileInfo
                {
                    Patient = patient,
                    Study = study,
                    Series = series,
                    Image = image
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }


        private (string lastName, string firstName, string middleName) ParseDicomName(string dicomName)
        {
            if (string.IsNullOrWhiteSpace(dicomName))
                return (string.Empty, string.Empty, string.Empty);

            var parts = dicomName.Split('^');
            return (
                lastName: parts.Length > 0 ? parts[0].Trim() : string.Empty,
                firstName: parts.Length > 1 ? parts[1].Trim() : string.Empty,
                middleName: parts.Length > 2 ? parts[2].Trim() : string.Empty
            );
        }

        private PatientSex ParsePatientSex(string sex)
        {
            if (string.IsNullOrWhiteSpace(sex))
                return PatientSex.Other;

            return sex.ToUpper() switch
            {
                "M" => PatientSex.Male,
                "F" => PatientSex.Female,
                "O" => PatientSex.Other,
                _ => PatientSex.Other
            };
        }

        private string CalculateAge(DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return string.Empty;

            var age = DateTime.Now.Year - birthDate.Value.Year;
            return $"{age:000}Y";
        }

        private static string GetProjection(DicomDataset dataset)
        {
            if (!dataset.Contains(DicomTag.ViewCodeSequence))
                return string.Empty;

            var item = dataset.GetSequence(DicomTag.ViewCodeSequence).Items.FirstOrDefault();
            return item == null
                ? string.Empty
                : item.GetStringValue(DicomTag.CodeMeaning, string.Empty);
        }

        private static string GetLaterality(DicomDataset dataset)
        {
            var laterality = dataset.GetStringValue(DicomTag.ImageLaterality, string.Empty);
            return string.IsNullOrWhiteSpace(laterality)
                ? dataset.GetStringValue(DicomTag.Laterality, string.Empty)
                : laterality;
        }

        private static double GetDose(DicomDataset dataset)
        {
            var generatedDose = new DicomTag(0x0031, 0x0201);
            if (dataset.TryGetSingleValue(generatedDose, out decimal dose))
                return (double)dose;

            return dataset.GetDoubleValue(DicomTag.EntranceDose) ?? 0;
        }

        private string[] GetBodyParts(DicomDataset dataset)
        {
            var bodyPart = dataset.GetStringValue(DicomTag.BodyPartExamined, string.Empty);
            return string.IsNullOrWhiteSpace(bodyPart)
                ? Array.Empty<string>()
                : new[] { bodyPart };
        }

        private DateTime? GetStudyDateTime(DicomDataset dataset)
        {
            var date = dataset.GetStringValue(DicomTag.StudyDate, string.Empty);
            var time = dataset.GetStringValue(DicomTag.StudyTime, string.Empty);

            if (string.IsNullOrWhiteSpace(date))
                return null;

            time = time?.Trim() ?? string.Empty;

            var value = string.IsNullOrWhiteSpace(time)
                ? date
                : $"{date} {time}";

            if (DateTime.TryParseExact(
                value,
                new[]
                {
                    "yyyyMMdd HHmmss",
                    "yyyyMMdd HHmmss.FFFFFF",
                    "yyyyMMdd"
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
            {
                return result;
            }

            return null;
        }

        private DateTime? GetSeriesDateTime(DicomDataset dataset)
        {
            var date = dataset.GetStringValue(DicomTag.SeriesDate, string.Empty)?.Trim();
            var time = dataset.GetStringValue(DicomTag.SeriesTime, string.Empty)?.Trim();

            if (string.IsNullOrWhiteSpace(date))
                date = dataset.GetStringValue(DicomTag.StudyDate, string.Empty)?.Trim();

            if (string.IsNullOrWhiteSpace(date))
                return null;

            time = time?.Trim() ?? string.Empty;

            var value = string.IsNullOrWhiteSpace(time)
                ? date
                : $"{date} {time}";

            if (DateTime.TryParseExact(
                value,
                new[]
                {
                    "yyyyMMdd HHmmss",
                    "yyyyMMdd HHmmss.FFFFFF",
                    "yyyyMMdd"
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
            {
                return result;
            }

            return null;
        }

        private DateTime? GetAcquisitionTime(DicomDataset dataset)
        {
            var timeStr = dataset.GetStringValue(DicomTag.AcquisitionTime);

            if (string.IsNullOrWhiteSpace(timeStr))
                return null;

            // Парсим время формата "HHmmss" или "HHmmss.fff"
            if (DateTime.TryParseExact(timeStr, new[] { "HHmmss", "HHmmss.fff" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
