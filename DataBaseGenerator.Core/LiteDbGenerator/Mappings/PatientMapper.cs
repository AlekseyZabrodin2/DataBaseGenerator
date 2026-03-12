using DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Mappings
{
    public static class PatientMapper
    {
        public static LitePatient ToLite(PatientLiteDb patient)
        {
            var entity = new LitePatient
            {
                PatientId = patient.PatientID,
                LastName = patient.LastName,
                FirstName = patient.FirstName,
                MiddleName = patient.MiddleName,
                Sex = patient.Sex,
                BirthDate = patient.BirthDate,
                Address = patient.Address,
                Phone = patient.Phone,
                Comments = patient.Comments
            };

            if (ObjectIdMapper.TryParse(patient.Id, out var objectId))
            {
                entity.Id = objectId;
            }

            return entity;
        }

        public static PatientLiteDb ToDomain(LitePatient entity)
        {
            return new PatientLiteDb
            {
                Id = ObjectIdMapper.ToString(entity.Id),
                PatientID = entity.PatientId,
                LastName = entity.LastName,
                FirstName = entity.FirstName,
                MiddleName = entity.MiddleName,
                Sex = entity.Sex,
                BirthDate = entity.BirthDate,
                Address = entity.Address,
                Phone = entity.Phone,
                Comments = entity.Comments
            };
        }
    }
}
