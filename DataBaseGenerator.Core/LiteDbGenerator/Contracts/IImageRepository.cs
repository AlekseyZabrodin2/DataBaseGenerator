using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Contracts
{
    public interface IImageRepository
    {
        ObservableCollection<ImageLiteDb> GetAllImage();
        ImageLiteDb Insert(ImageLiteDb image); 
        void InsertBulk(IEnumerable<ImageLiteDb> images);
        void Update(ImageLiteDb image);

        ImageLiteDb? GetById(string id);

        ImageLiteDb? FindBySopInstanceUid(string sopInstanceUid);

        IEnumerable<ImageLiteDb> FindByStudy(string studyInstanceUid, int limit);
        IEnumerable<ImageLiteDb> FindByBodyPart(string bodyPart, int limit);
        IEnumerable<ImageLiteDb> FindByProjection(string projection, int limit);
        IEnumerable<ImageLiteDb> FindByLaterality(string laterality, int limit);
        IEnumerable<ImageLiteDb> FindByAcquisitionDateRange(DateTime? from, DateTime? to, int limit);

        void DeleteAll();
        void DeleteById(string id);
        void DeleteBySopInstanceUid(string sopInstanceUid); 
        bool Any();
    }
}
