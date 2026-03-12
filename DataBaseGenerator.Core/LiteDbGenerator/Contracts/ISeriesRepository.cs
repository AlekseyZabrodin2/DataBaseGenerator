using System;
using System.Collections.Generic;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Contracts
{
    public interface ISeriesRepository
    {
        SeriesLiteDb Insert(SeriesLiteDb series);
        void Update(SeriesLiteDb series);

        SeriesLiteDb? GetById(string id);

        SeriesLiteDb? FindBySeriesInstanceUid(string seriesInstanceUid);

        IEnumerable<SeriesLiteDb> FindByStudy(string studyInstanceUid, int limit);
        IEnumerable<SeriesLiteDb> FindByModality(string modality, int limit);
        IEnumerable<SeriesLiteDb> FindByOperatorName(string operatorName, int limit);
        IEnumerable<SeriesLiteDb> FindBySeriesDateRange(DateTime? from, DateTime? to, int limit);

        void DeleteById(string id);
        void DeleteBySeriesInstanceUid(string seriesInstanceUid);
    }
}
