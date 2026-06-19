using System;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Study;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.Models
{
    public class StudyGeneratorDto
    {
        public StudyGeneratorDto(
            RandomAccessionNumberRule accessionNumber,
            RandomBodyPartsRule bodyParts,
            RandomStudyDateTimeRule studyDateTime,
            RandomEffectiveDoseRule effectiveDose,
            RandomStatusRule status)
        {
            RandomAccessionNumber = accessionNumber ?? throw new ArgumentNullException(nameof(accessionNumber));
            RandomBodyParts = bodyParts ?? throw new ArgumentNullException(nameof(bodyParts));
            RandomStudyDateTime = studyDateTime ?? throw new ArgumentNullException(nameof(studyDateTime));
            RandomEffectiveDose = effectiveDose ?? throw new ArgumentNullException(nameof(effectiveDose));
            RandomStatus = status ?? throw new ArgumentNullException(nameof(status));
        }


        public int StudyCount { get; set; }
        public ObjectId Id { get; set; }
        public string StudyInstanceUid { get; set; }
        public string StudyId { get; set; }
        public ObjectId PatientId { get; set; }
        public string SnapshotLastName { get; set; }
        public string SnapshotFirstName { get; set; }
        public string SnapshotPatronymic { get; set; }
        public string SnapshotPatientId { get; set; }
        public DateTime? PatientBirthDate { get; set; }
        public string AccessionNumber { get; set; }
        public string[] BodyParts { get; set; }
        public DateTime StudyDateTime { get; set; }
        public double EffectiveDosemSv { get; set; }
        public string Status { get; set; }

        public RandomAccessionNumberRule RandomAccessionNumber { get; }
        public RandomBodyPartsRule RandomBodyParts { get; }
        public RandomStudyDateTimeRule RandomStudyDateTime { get; }
        public RandomEffectiveDoseRule RandomEffectiveDose { get; }
        public RandomStatusRule RandomStatus { get; }
    }
}
