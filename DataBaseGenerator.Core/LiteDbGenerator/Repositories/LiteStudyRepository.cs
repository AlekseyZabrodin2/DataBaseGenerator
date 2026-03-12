using System;
using System.Collections.Generic;
using DataBaseGenerator.Core.LiteDbData.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Repositories
{
    public sealed class LiteStudyRepository : IStudyRepository
    {
        private readonly LiteDbContext _context;

        public LiteStudyRepository(LiteDbContext context)
        {
            _context = context;
        }

        public void DeleteById(string id)
        {
            throw new NotImplementedException();
        }

        public void DeleteByStudyInstanceUid(string uid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindByEffectiveDoseGreaterOrEqual(double dose, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindByPatient(string patientId, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindBySnapshotFirstName(string firstName, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindBySnapshotLastName(string lastName, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindBySnapshotPatronymic(string patronymic, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindByStudyDateFrom(DateTime from, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindByStudyDateRange(DateTime? from, DateTime? to, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindByStudyDescription(string description, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindByStudyId(string studyId, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindByStudyInstanceUid(string uid, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudyLiteDb> FindByStudyInstanceUids(IEnumerable<string> uids)
        {
            throw new NotImplementedException();
        }

        public StudyLiteDb GetById(string id)
        {
            throw new NotImplementedException();
        }

        public StudyLiteDb Insert(StudyLiteDb study)
        {
            throw new NotImplementedException();
        }

        public void Update(StudyLiteDb study)
        {
            throw new NotImplementedException();
        }
    }
}
