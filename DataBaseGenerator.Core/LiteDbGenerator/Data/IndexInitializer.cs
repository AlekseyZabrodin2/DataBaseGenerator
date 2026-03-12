using DataBaseGenerator.Core.LiteDbData.DbContext;

namespace DataBaseGenerator.Core.LiteDbGenerator.Data
{
    public static class IndexInitializer
    {
        public static void EnsureIndexes(LiteDbContext context)
        {
            context.Patients.EnsureIndex(x => x.PatientId);
            context.Patients.EnsureIndex(x => x.LastName);
            context.Patients.EnsureIndex(x => x.FirstName);
            context.Patients.EnsureIndex(x => x.MiddleName);
            context.Patients.EnsureIndex(x => x.BirthDate);
            context.Patients.EnsureIndex(x => x.Phone);
            context.Patients.EnsureIndex(x => x.Address);

            context.Studies.EnsureIndex(x => x.StudyInstanceUid, true);
            context.Studies.EnsureIndex(x => x.StudyId);
            context.Studies.EnsureIndex(x => x.PatientId);
            context.Studies.EnsureIndex(x => x.SnapshotLastName);
            context.Studies.EnsureIndex(x => x.SnapshotFirstName);
            context.Studies.EnsureIndex(x => x.SnapshotPatronymic);
            context.Studies.EnsureIndex(x => x.AccessionNumber);
            context.Studies.EnsureIndex(x => x.StudyDateTime);
            context.Studies.EnsureIndex(x => x.Status);
            context.Studies.EnsureIndex(x => x.EffectiveDosemSv);

            context.Series.EnsureIndex(x => x.SeriesInstanceUid, true);
            context.Series.EnsureIndex(x => x.StudyInstanceUid);
            context.Series.EnsureIndex(x => x.Modality);
            context.Series.EnsureIndex(x => x.BodyPartExamined);
            context.Series.EnsureIndex(x => x.OperatorName);
            context.Series.EnsureIndex(x => x.SeriesDateTime);

            context.Images.EnsureIndex(x => x.SopInstanceUid, true);
            context.Images.EnsureIndex(x => x.StudyInstanceUid);
            context.Images.EnsureIndex(x => x.SeriesInstanceUid);
            context.Images.EnsureIndex(x => x.BodyPart);
            context.Images.EnsureIndex(x => x.Projection);
            context.Images.EnsureIndex(x => x.Laterality);
            context.Images.EnsureIndex(x => x.InstanceDose);
            context.Images.EnsureIndex(x => x.AcquisitionTime);
        }
    }
}
