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
    public sealed class LiteSeriesRepository : ISeriesRepository
    {
        private readonly LiteDbContext _context; 
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public LiteSeriesRepository(LiteDbContext context)
        {
            _context = context;
        }


        public ObservableCollection<SeriesLiteDb> GetAllSeries()
        {
            try
            {
                var series = _context.Series.FindAll().ToList();

                _logger.Info($"Загружено серий: {series.Count}");

                var serieCollection = new ObservableCollection<SeriesLiteDb>(
                series.Select(serie => new SeriesLiteDb
                {
                    Id = serie.Id,
                    SeriesInstanceUid = serie.SeriesInstanceUid,
                    StudyInstanceUid = serie.StudyInstanceUid,
                    OperatorName = serie.OperatorName,
                    Modality = serie.Modality,
                    BodyPartExamined = serie.BodyPartExamined,
                    SeriesDateTime = serie.SeriesDateTime
                })
            );

                return serieCollection;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении серий: {ex.Message}");
                return new ObservableCollection<SeriesLiteDb>();
            }
        }

        public SeriesLiteDb Insert(SeriesLiteDb series)
        {
            var entity = new LiteSeries
            {
                Id = series.Id,
                SeriesInstanceUid = series.SeriesInstanceUid,
                StudyInstanceUid = series.StudyInstanceUid,
                OperatorName = series.OperatorName,
                Modality = series.Modality,
                BodyPartExamined = series.BodyPartExamined,
                SeriesDateTime = series.SeriesDateTime
            };

            _context.Series.Upsert(entity);

            series.Id = entity.Id;

            return series;
        }

        public void InsertBulk(IEnumerable<SeriesLiteDb> series)
        {
            var entities = new List<LiteSeries>();
            foreach (var serie in series)
            {
                var entity = new LiteSeries
                {
                    Id = serie.Id,
                    SeriesInstanceUid = serie.SeriesInstanceUid,
                    StudyInstanceUid = serie.StudyInstanceUid,
                    OperatorName = serie.OperatorName,
                    Modality = serie.Modality,
                    BodyPartExamined = serie.BodyPartExamined,
                    SeriesDateTime = serie.SeriesDateTime
                };
                entities.Add(entity);
            }

            _context.Series.InsertBulk(entities);
        }

        public void DeleteAll()
        {
            _context.Series.DeleteAll();
        }

        public void DeleteById(string id)
        {
            throw new NotImplementedException();
        }

        public void DeleteBySeriesInstanceUid(string seriesInstanceUid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SeriesLiteDb> FindByModality(string modality, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SeriesLiteDb> FindByOperatorName(string operatorName, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SeriesLiteDb> FindBySeriesDateRange(DateTime? from, DateTime? to, int limit)
        {
            throw new NotImplementedException();
        }

        public SeriesLiteDb FindBySeriesInstanceUid(string seriesInstanceUid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SeriesLiteDb> FindByStudy(string studyInstanceUid, int limit)
        {
            throw new NotImplementedException();
        }

        public SeriesLiteDb GetById(string id)
        {
            throw new NotImplementedException();
        }

        public void Update(SeriesLiteDb series)
        {
            throw new NotImplementedException();
        }

        public bool Any()
        {
            return _context.Series.Count() > 0;
        }
    }
}
