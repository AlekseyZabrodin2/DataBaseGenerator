using System;
using System.Linq;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels
{
    public sealed class LitePlannedStudy
    {
        [BsonId]
        public string Id { get; set; }

        public string PatientLastName { get; set; }

        public string PatientFirstName { get; set; }

        public string PatientMiddleName { get; set; }

        public DateTime? BirthDate { get; set; }

        public PatientSex Gender { get; set; }

        public string Telephone { get; set; }

        public string Address { get; set; }

        public string Age { get; set; }

        public string Comments { get; set; }

        public string PatientId { get; set; }

        public string StudyID { get; set; }

        public string AccessionNumber { get; set; }

        public string PatientIdDataBase { get; set; }

        public string StudyInstanceUid { get; set; }

        public PlannedStudyArea[] StudyAreas { get; set; }

        [BsonIgnore]
        public string StudyAreasDisplay => FormatArrayDisplay(StudyAreas);

        public string[] ProjectionPaths { get; set; }
        [BsonIgnore]
        public string ProjectionPathsDisplay => FormatArrayDisplay(ProjectionPaths);

        public PlannedStudyStatus Status { get; set; }

        public int? ImagesCount { get; set; }

        private static string FormatArrayDisplay<T>(T[] array)
        {
            if (array == null)
                return string.Empty;
            return array.Length == 1
                ? "[1 элемент]"
                : $"[{array.Length} элементов]";
        }

        [BsonIgnore]
        public string StudyAreasTooltip => StudyAreas != null
            ? string.Join(Environment.NewLine, StudyAreas.Select(a => a?.ToString() ?? string.Empty))
            : string.Empty;

        [BsonIgnore]
        public string ProjectionPathsTooltip => ProjectionPaths != null
            ? string.Join(Environment.NewLine, ProjectionPaths)
            : string.Empty;


        public static LitePlannedStudy FromItem(PlannedStudyLiteDb item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return new LitePlannedStudy
            {
                Id = BuildKey(item),
                PatientLastName = item.PatientLastName ?? string.Empty,
                PatientFirstName = item.PatientFirstName ?? string.Empty,
                PatientMiddleName = item.PatientMiddleName ?? string.Empty,
                BirthDate = item.BirthDate,
                Gender = item.Gender,
                Telephone = item.Telephone ?? string.Empty,
                Address = item.Address ?? string.Empty,
                Age = item.Age ?? string.Empty,
                Comments = item.Comments ?? string.Empty,
                PatientId = item.PatientId ?? string.Empty,
                StudyID = item.StudyID ?? string.Empty,
                AccessionNumber = item.AccessionNumber ?? string.Empty,
                PatientIdDataBase = item.PatientIdDataBase ?? string.Empty,
                StudyInstanceUid = item.StudyInstanceUid ?? string.Empty,
                StudyAreas = item.StudyAreas?.ToArray() ?? Array.Empty<PlannedStudyArea>(),
                ProjectionPaths = item.ProjectionPaths?.ToArray() ?? Array.Empty<string>(),
                Status = item.Status,
                ImagesCount = item.ImagesCount ?? 0
            };
        }

        private static string BuildKey(PlannedStudyLiteDb item)
        {
            return item.StudyInstanceUid ?? string.Empty;
        }
    }
}
