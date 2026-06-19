using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Contracts
{
    public interface IPatientRepository
    {
        // Insert or update patient; implementation should return entity with storage Id set
        ObservableCollection<PatientLiteDb> GetAllPatients();
        PatientLiteDb Upsert(PatientLiteDb patient);
        void InsertBulk(IEnumerable<PatientLiteDb> patients);

        PatientLiteDb? GetById(string id);

        PatientLiteDb? FindByPatientId(string patientId);

        IEnumerable<PatientLiteDb> FindByLastName(string lastName, int limit);
        IEnumerable<PatientLiteDb> FindByFirstName(string firstName, int limit);
        IEnumerable<PatientLiteDb> FindByMiddleName(string middleName, int limit);

        // Combined search by first and last name; null values mean "do not filter by this field".
        IEnumerable<PatientLiteDb> FindByName(string? firstName, string? lastName, int limit);

        IEnumerable<PatientLiteDb> FindByBirthDateRange(DateTime? from, DateTime? to, int limit);
        IEnumerable<PatientLiteDb> FindByAddress(string addressPart, int limit);
        IEnumerable<PatientLiteDb> FindByPhone(string phone, int limit);

        void Delete(string id);
        void DeleteAll(); 
        bool Any();
    }
}
