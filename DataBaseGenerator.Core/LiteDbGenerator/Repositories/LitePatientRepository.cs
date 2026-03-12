using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataBaseGenerator.Core.LiteDbData.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels;
using DataBaseGenerator.Core.LiteDbGenerator.Mappings;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.Repositories
{
    public sealed class LitePatientRepository : IPatientRepository
    {
        private readonly LiteDbContext _context;

        public LitePatientRepository(LiteDbContext context)
        {
            _context = context;
        }

        public ObservableCollection<PatientLiteDb> GetAllPatients()
        {
            try
            {
                var patientCollection = new ObservableCollection<PatientLiteDb>();
                var patients = _context.Patients.FindAll().ToList();

                foreach (var patient in patients)
                {
                    var convertPatient = ConvertPatientToPatientLiteDb(patient);
                    patientCollection.Add(convertPatient);
                }

                Console.WriteLine($"Найдено пациентов: {patients.Count}");
                return patientCollection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пациентов: {ex.Message}");
                return new ObservableCollection<PatientLiteDb>();
            }
        }

        private PatientLiteDb ConvertPatientToPatientLiteDb(LitePatient patient, string patientId = null)
        {
            return new PatientLiteDb()
            {
                Id = patientId,
                PatientID = patient.PatientId,
                LastName = patient.LastName,
                FirstName = patient.FirstName,
                MiddleName = patient.MiddleName,
                BirthDate = patient.BirthDate,
                Sex = patient.Sex,
                Phone = patient.Phone,
                Address = patient.Address,
                Comments = patient.Comments
                //Age = patient.Age
            };
        }

        public PatientLiteDb Upsert(PatientLiteDb patient)
        {
            var entity = PatientMapper.ToLite(patient);

            // Preserve existing document by PatientId if no internal id is set
            if (entity.Id == ObjectId.Empty)
            {
                var existing = _context.Patients.FindOne(p => p.PatientId == entity.PatientId);
                if (existing is not null)
                {
                    entity.Id = existing.Id;
                }
            }

            _context.Patients.Upsert(entity);

            return PatientMapper.ToDomain(entity);
        }

        public PatientLiteDb? GetById(string id)
        {
            if (!ObjectIdMapper.TryParse(id, out var objectId))
                return null;

            var entity = _context.Patients.FindById(objectId);
            return entity is null ? null : PatientMapper.ToDomain(entity);
        }

        public PatientLiteDb? FindByPatientId(string patientId)
        {
            var entity = _context.Patients.FindOne(p => p.PatientId == patientId);
            return entity is null ? null : PatientMapper.ToDomain(entity);
        }

        public IEnumerable<PatientLiteDb> FindByLastName(string lastName, int limit) =>
            _context.Patients
                .Query()
                .Where(p => p.LastName.StartsWith(lastName))
                .Limit(limit <= 0 ? int.MaxValue : limit)
                .ToEnumerable()
                .Select(PatientMapper.ToDomain);

        public IEnumerable<PatientLiteDb> FindByFirstName(string firstName, int limit) =>
            _context.Patients
                .Query()
                .Where(p => p.FirstName.StartsWith(firstName))
                .Limit(limit <= 0 ? int.MaxValue : limit)
                .ToEnumerable()
                .Select(PatientMapper.ToDomain);

        public IEnumerable<PatientLiteDb> FindByMiddleName(string middleName, int limit) =>
            _context.Patients
                .Query()
                .Where(p => p.MiddleName.StartsWith(middleName))
                .Limit(limit <= 0 ? int.MaxValue : limit)
                .ToEnumerable()
                .Select(PatientMapper.ToDomain);

        public IEnumerable<PatientLiteDb> FindByName(string? firstName, string? lastName, int limit)
        {
            var query = _context.Patients.Query();

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(p => p.FirstName.StartsWith(firstName));
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(p => p.LastName.StartsWith(lastName));
            }

            return query
                .Limit(limit <= 0 ? int.MaxValue : limit)
                .ToEnumerable()
                .Select(PatientMapper.ToDomain);
        }

        public IEnumerable<PatientLiteDb> FindByBirthDateRange(DateTime? from, DateTime? to, int limit) =>
            _context.Patients
                .Query()
                .Where(p =>
                    (!from.HasValue || (p.BirthDate.HasValue && p.BirthDate.Value >= from.Value)) &&
                    (!to.HasValue || (p.BirthDate.HasValue && p.BirthDate.Value <= to.Value)))
                .Limit(limit <= 0 ? int.MaxValue : limit)
                .ToEnumerable()
                .Select(PatientMapper.ToDomain);

        public IEnumerable<PatientLiteDb> FindByAddress(string addressPart, int limit) =>
            _context.Patients
                .Query()
                .Where(p => p.Address.Contains(addressPart))
                .Limit(limit <= 0 ? int.MaxValue : limit)
                .ToEnumerable()
                .Select(PatientMapper.ToDomain);

        public IEnumerable<PatientLiteDb> FindByPhone(string phone, int limit) =>
            _context.Patients
                .Query()
                .Where(p => p.Phone.Contains(phone))
                .Limit(limit <= 0 ? int.MaxValue : limit)
                .ToEnumerable()
                .Select(PatientMapper.ToDomain);

        public void Delete(string id)
        {
            if (!ObjectIdMapper.TryParse(id, out var objectId))
                return;

            _context.Patients.Delete(objectId);
        }
    }
}
