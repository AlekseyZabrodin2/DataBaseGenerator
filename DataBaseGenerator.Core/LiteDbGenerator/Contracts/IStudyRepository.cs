using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Contracts
{
    public interface IStudyRepository
    {
        ObservableCollection<StudyLiteDb> GetAllStudies();
        StudyLiteDb Insert(StudyLiteDb study);
        void Update(StudyLiteDb study);
        StudyLiteDb Upsert(StudyLiteDb patient);
        void InsertBulk(IEnumerable<StudyLiteDb> patients);

        StudyLiteDb? GetById(string id);

        IEnumerable<StudyLiteDb> FindByPatient(string patientId, int limit);
        IEnumerable<StudyLiteDb> FindByStudyDateFrom(DateTime from, int limit);
        IEnumerable<StudyLiteDb> FindByStudyDateRange(DateTime? from, DateTime? to, int limit);

        IEnumerable<StudyLiteDb> FindByStudyInstanceUid(string uid, int limit);
        IEnumerable<StudyLiteDb> FindByStudyInstanceUids(IEnumerable<string> uids);
        IEnumerable<StudyLiteDb> FindByStudyId(string studyId, int limit);
        IEnumerable<StudyLiteDb> FindByStudyDescription(string description, int limit);

        IEnumerable<StudyLiteDb> FindBySnapshotLastName(string lastName, int limit);
        IEnumerable<StudyLiteDb> FindBySnapshotFirstName(string firstName, int limit);
        IEnumerable<StudyLiteDb> FindBySnapshotPatronymic(string patronymic, int limit);

        IEnumerable<StudyLiteDb> FindByEffectiveDoseGreaterOrEqual(double dose, int limit);

        void DeleteById(string id);
        void DeleteByStudyInstanceUid(string uid);
        void DeleteAll();
        bool Any();
    }
}
