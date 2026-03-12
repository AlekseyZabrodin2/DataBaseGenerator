using System;
using System.Collections.Generic;
using DataBaseGenerator.Core.LiteDbData.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Repositories
{
    public sealed class LiteSeriesRepository : ISeriesRepository
    {
        private readonly LiteDbContext _context;

        public LiteSeriesRepository(LiteDbContext context)
        {
            _context = context;
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

        public SeriesLiteDb Insert(SeriesLiteDb series)
        {
            throw new NotImplementedException();
        }

        public void Update(SeriesLiteDb series)
        {
            throw new NotImplementedException();
        }
    }
}
