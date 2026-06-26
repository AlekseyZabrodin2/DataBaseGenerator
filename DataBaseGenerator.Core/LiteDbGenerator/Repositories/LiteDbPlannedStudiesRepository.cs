using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using LiteDB;
using NLog;

namespace DataBaseGenerator.Core.LiteDbGenerator.Repositories
{
    public sealed class LiteDbPlannedStudiesRepository : IPlannedStudiesRepository
    {
        private readonly LiteDbPlannedStudiesContext _context;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


        public LiteDbPlannedStudiesRepository(LiteDbPlannedStudiesContext context)
        {
            _context = context;
        }



        public PlannedStudyLiteDb GetById(string id)
        {
            return _context.PlannedStudies.FindById(new ObjectId(id));
        }

        public void Insert(PlannedStudyLiteDb study)
        {
            _context.PlannedStudies.Insert(study);
        }

        public void InsertBulk(IEnumerable<PlannedStudyLiteDb> studies)
        {
            _context.PlannedStudies.InsertBulk(studies);
        }

        public void Update(PlannedStudyLiteDb study)
        {
            _context.PlannedStudies.Update(study);
        }

        public void Delete(string id)
        {
            _context.PlannedStudies.Delete(new ObjectId(id));
        }

        public void DeleteAll()
        {
            _context.PlannedStudies.DeleteAll();
        }

        public bool Any()
        {
            return _context.PlannedStudies.Count() > 0;
        }

        public ObservableCollection<LitePlannedStudy> GetAllStudies()
        {
            try
            {
                var plannedStudyCollection = new ObservableCollection<LitePlannedStudy>();
                var studies = _context.PlannedStudies.FindAll().ToList();

                foreach (var study in studies)
                {
                    var convertPatient = ConvertPlannedStudyToPlannedStudyLiteDb(study);
                    plannedStudyCollection.Add(convertPatient);
                }

                _logger.Info($"Найдено исследований: {studies.Count}");
                return plannedStudyCollection;
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при получении исследований: {ex.Message}");
                return new ObservableCollection<LitePlannedStudy>();
            }
        }
        

        public LitePlannedStudy ConvertPlannedStudyToPlannedStudyLiteDb(PlannedStudyLiteDb plannedStudy)
        {
            return new LitePlannedStudy()
            {
                Id = plannedStudy.Id,
                PatientLastName = plannedStudy.PatientLastName ?? string.Empty,
                PatientFirstName = plannedStudy.PatientFirstName ?? string.Empty,
                PatientMiddleName = plannedStudy.PatientMiddleName ?? string.Empty,
                BirthDate = plannedStudy.BirthDate,
                Gender = plannedStudy.Gender,
                Telephone = plannedStudy.Telephone ?? string.Empty,
                Address = plannedStudy.Address ?? string.Empty,
                Age = plannedStudy.Age ?? string.Empty,
                Comments = plannedStudy.Comments ?? string.Empty,
                PatientId = plannedStudy.PatientId ?? string.Empty,
                StudyID = plannedStudy.StudyID ?? string.Empty,
                AccessionNumber = plannedStudy.AccessionNumber ?? string.Empty,
                PatientIdDataBase = plannedStudy.PatientIdDataBase ?? string.Empty,
                StudyInstanceUid = plannedStudy.StudyInstanceUid ?? string.Empty,
                StudyAreas = plannedStudy.StudyAreas ?? Array.Empty<PlannedStudyArea>(),
                ProjectionPaths = plannedStudy.ProjectionPaths ?? Array.Empty<string>(),
                Status = plannedStudy.Status,
                ImagesCount = plannedStudy.ImagesCount ?? 0
            };
        }
    }
}
