using System;
using System.Collections.Generic;
using System.Linq;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Services
{
    public sealed class StudyApplicationService : IStudyApplicationService
    {
        private readonly IStudyStorageModule _storage;

        public StudyApplicationService(IStudyStorageModule storage)
        {
            _storage = storage;
        }

        public PatientLiteDb UpsertPatient(PatientLiteDb patient)
        {
            return _storage.Patients.Upsert(patient);
        }

        /// limit - maximum number of records to return, default is 0 (MaxValue). If set to int.MaxValue, all matching records will be returned.
        public IReadOnlyList<PatientLiteDb> SearchPatientsByLastName(string lastName, int limit = 0) =>
            _storage.Patients.FindByLastName(lastName, limit).ToList();

        public IReadOnlyList<PatientLiteDb> SearchPatientsByFirstName(string firstName, int limit = 0) =>
            _storage.Patients.FindByFirstName(firstName, limit).ToList();

        public IReadOnlyList<PatientLiteDb> SearchPatientsByMiddleName(string middleName, int limit = 0) =>
            _storage.Patients.FindByMiddleName(middleName, limit).ToList();

        public IReadOnlyList<PatientLiteDb> SearchPatientsByBirthDateRange(DateTime? from, DateTime? to, int limit = 0) =>
            _storage.Patients.FindByBirthDateRange(from, to, limit).ToList();

        public IReadOnlyList<PatientLiteDb> SearchPatientsByAddress(string addressPart, int limit = 0) =>
            _storage.Patients.FindByAddress(addressPart, limit).ToList();

        public IReadOnlyList<PatientLiteDb> SearchPatientsByPhone(string phone, int limit = 0) =>
            _storage.Patients.FindByPhone(phone, limit).ToList();

        public IReadOnlyList<PatientLiteDb> SearchPatientsByName(string? firstName, string? lastName, int limit = 0) =>
            _storage.Patients.FindByName(firstName, lastName, limit).ToList();
               

        public bool DeletePatientWithRelations(string patientId)
        {
            //var patient = _storage.Patients.FindByPatientId(patientId);
            //if (patient is null || string.IsNullOrWhiteSpace(patient.Id))
            //    return false;

            //var studies = _storage.Studies.FindByPatient(patient.Id, int.MaxValue).ToList();
            //foreach (var study in studies)
            //{
            //    DeleteStudyWithRelations(study.StudyInstanceUid);
            //}

            //_storage.Patients.Delete(patient.Id);
            return true;
        }

    }
}
