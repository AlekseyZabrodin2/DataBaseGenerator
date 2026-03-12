using System;
using System.Collections.Generic;
using DataBaseGenerator.Core.LiteDbData.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Models;

namespace DataBaseGenerator.Core.LiteDbGenerator.Repositories
{
    public sealed class LiteImageRepository : IImageRepository
    {
        private readonly LiteDbContext _context;

        public LiteImageRepository(LiteDbContext context)
        {
            _context = context;
        }

        public void DeleteById(string id)
        {
            throw new NotImplementedException();
        }

        public void DeleteBySopInstanceUid(string sopInstanceUid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ImageLiteDb> FindByAcquisitionDateRange(DateTime? from, DateTime? to, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ImageLiteDb> FindByBodyPart(string bodyPart, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ImageLiteDb> FindByLaterality(string laterality, int limit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ImageLiteDb> FindByProjection(string projection, int limit)
        {
            throw new NotImplementedException();
        }

        public ImageLiteDb FindBySopInstanceUid(string sopInstanceUid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ImageLiteDb> FindByStudy(string studyInstanceUid, int limit)
        {
            throw new NotImplementedException();
        }

        public ImageLiteDb GetById(string id)
        {
            throw new NotImplementedException();
        }

        public ImageLiteDb Insert(ImageLiteDb image)
        {
            throw new NotImplementedException();
        }

        public void Update(ImageLiteDb image)
        {
            throw new NotImplementedException();
        }
    }
}
