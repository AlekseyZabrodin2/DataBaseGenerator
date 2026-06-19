using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataBaseGenerator.Core.LiteDbData.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using LiteDB;
using NLog;

namespace DataBaseGenerator.Core.LiteDbGenerator.Repositories
{
    public sealed class LiteStudyRepository : IStudyRepository
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly LiteDbContext _context;

        public LiteStudyRepository(LiteDbContext context)
        {
            _context = context;
        }


        public ObservableCollection<StudyLiteDb> GetAllStudies()
        {
            try
            {
                var studies = _context.Studies.FindAll().ToList();

                _logger.Info($"Загружено исследований: {studies.Count}");

                var studyCollection = new ObservableCollection<StudyLiteDb>(
                studies.Select(study => new StudyLiteDb
                {
                    Id = study.Id,
                    StudyInstanceUid = study.StudyInstanceUid,
                    StudyId = study.StudyId,
                    PatientId = study.PatientId,
                    SnapshotLastName = study.SnapshotLastName,
                    SnapshotFirstName = study.SnapshotFirstName,
                    SnapshotPatronymic = study.SnapshotPatronymic,
                    SnapshotPatientId = study.SnapshotPatientId,
                    PatientBirthDate = study.PatientBirthDate,
                    AccessionNumber = study.AccessionNumber,
                    BodyParts = study.BodyParts,
                    StudyDateTime = study.StudyDateTime,
                    EffectiveDosemSv = study.EffectiveDosemSv,
                    Status = study.Status
                })
            );

                return studyCollection;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении исследований: {ex.Message}");
                return new ObservableCollection<StudyLiteDb>();
            }
        }

        private StudyLiteDb ConvertLiteStudyToStudyLiteDb(LiteStudy study)
        {
            return new StudyLiteDb()
            {
                Id = study.Id,
                StudyInstanceUid = study.StudyInstanceUid,
                StudyId = study.StudyId,
                PatientId = study.PatientId,
                SnapshotLastName = study.SnapshotLastName,
                SnapshotFirstName = study.SnapshotFirstName,
                SnapshotPatronymic = study.SnapshotPatronymic,
                SnapshotPatientId = study.SnapshotPatientId,
                PatientBirthDate = study.PatientBirthDate,
                AccessionNumber = study.AccessionNumber,
                BodyParts = study.BodyParts,
                StudyDateTime = study.StudyDateTime,
                EffectiveDosemSv = study.EffectiveDosemSv,
                Status = study.Status
            };
        }

        public StudyLiteDb Upsert(StudyLiteDb stydy)
        {
            var entity = new LiteStudy
            {
                StudyInstanceUid = stydy.StudyInstanceUid,
                StudyId = stydy.StudyId,
                PatientId = stydy.PatientId,
                SnapshotLastName = stydy.SnapshotLastName,
                SnapshotFirstName = stydy.SnapshotFirstName,
                SnapshotPatronymic = stydy.SnapshotPatronymic,
                SnapshotPatientId = stydy.SnapshotPatientId,
                PatientBirthDate = stydy.PatientBirthDate,
                AccessionNumber = stydy.AccessionNumber,
                BodyParts = stydy.BodyParts,
                StudyDateTime = stydy.StudyDateTime,
                EffectiveDosemSv = stydy.EffectiveDosemSv,
                Status = stydy.Status
            };

            _context.Studies.Upsert(entity);

            stydy.Id = entity.Id;

            return stydy;
        }

        public void InsertBulk(IEnumerable<StudyLiteDb> stydies)
        {
            var entities = new List<LiteStudy>();
            foreach (var stydy in stydies)
            {
                var entity = new LiteStudy
                {
                    StudyInstanceUid = stydy.StudyInstanceUid,
                    StudyId = stydy.StudyId,
                    PatientId = stydy.PatientId,
                    SnapshotLastName = stydy.SnapshotLastName,
                    SnapshotFirstName = stydy.SnapshotFirstName,
                    SnapshotPatronymic = stydy.SnapshotPatronymic,
                    SnapshotPatientId = stydy.SnapshotPatientId,
                    PatientBirthDate = stydy.PatientBirthDate,
                    AccessionNumber = stydy.AccessionNumber,
                    BodyParts = stydy.BodyParts,
                    StudyDateTime = stydy.StudyDateTime,
                    EffectiveDosemSv = stydy.EffectiveDosemSv,
                    Status = stydy.Status
                };
                entities.Add(entity);
            }

            _context.Studies.InsertBulk(entities);
        }

        public void DeleteAll()
        {
            _context.Studies.DeleteAll();
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

        public bool Any()
        {
            return _context.Studies.Count() > 0;
        }
    }
}
