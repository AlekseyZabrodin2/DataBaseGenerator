using System;
using System.Linq;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.PlannedStudy;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.Models
{
    public class PlannedStudyLiteDbDto
    {
        public PlannedStudyLiteDbDto(RandomBodyPartDataRule randomBodyPart,
            RandomProjectionPathsRule randomProjectionPaths,
            RandomImagesCountRule randomImagesCount)
        {
            RandomBodyPart = randomBodyPart ?? throw new ArgumentNullException(nameof(randomBodyPart));
            RandomProjectionPaths = randomProjectionPaths ?? throw new ArgumentNullException(nameof(randomProjectionPaths));
            RandomImagesCount = randomImagesCount ?? throw new ArgumentNullException(nameof(randomImagesCount));
        }

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
        public string StudyAreasDisplay => StudyAreas != null
            ? string.Join(", ", StudyAreas.Select(a => a?.ToString() ?? string.Empty))
            : string.Empty;

        public string[] ProjectionPaths { get; set; }
        [BsonIgnore]
        public string ProjectionPathsDisplay => ProjectionPaths != null ? string.Join(", ", ProjectionPaths) : string.Empty;

        public string Status { get; set; }
        public string ImagesCount { get; set; }

        public RandomBodyPartDataRule RandomBodyPart { get; }
        public RandomProjectionPathsRule RandomProjectionPaths {  get; }
        public RandomImagesCountRule RandomImagesCount { get; }
    }
}
