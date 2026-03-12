using System;
using System.Collections.Generic;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Contracts
{
    public interface IImageRepository
    {
        ImageLiteDb Insert(ImageLiteDb image);
        void Update(ImageLiteDb image);

        ImageLiteDb? GetById(string id);

        ImageLiteDb? FindBySopInstanceUid(string sopInstanceUid);

        IEnumerable<ImageLiteDb> FindByStudy(string studyInstanceUid, int limit);
        IEnumerable<ImageLiteDb> FindByBodyPart(string bodyPart, int limit);
        IEnumerable<ImageLiteDb> FindByProjection(string projection, int limit);
        IEnumerable<ImageLiteDb> FindByLaterality(string laterality, int limit);
        IEnumerable<ImageLiteDb> FindByAcquisitionDateRange(DateTime? from, DateTime? to, int limit);

        void DeleteById(string id);
        void DeleteBySopInstanceUid(string sopInstanceUid);
    }
}
