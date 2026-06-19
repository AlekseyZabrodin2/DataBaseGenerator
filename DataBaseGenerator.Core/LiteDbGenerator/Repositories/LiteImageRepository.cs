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
using static System.Net.Mime.MediaTypeNames;

namespace DataBaseGenerator.Core.LiteDbGenerator.Repositories
{
    public sealed class LiteImageRepository : IImageRepository
    {
        private readonly LiteDbContext _context;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public LiteImageRepository(LiteDbContext context)
        {
            _context = context;
        }



        public ObservableCollection<ImageLiteDb> GetAllImage()
        {
            try
            {
                var images = _context.Images.FindAll().ToList();

                _logger.Info($"Загружено изображений: {images.Count}");

                var studyCollection = new ObservableCollection<ImageLiteDb>(
                images.Select(image => new ImageLiteDb
                {
                    Id = image.Id,
                    SopInstanceUid = image.SopInstanceUid,
                    SeriesInstanceUid = image.SeriesInstanceUid,
                    StudyInstanceUid = image.StudyInstanceUid,
                    BodyPart = image.BodyPart,
                    Projection = image.Projection,
                    Laterality = image.Laterality,
                    InstanceDose = image.InstanceDose,
                    AcquisitionTime = image.AcquisitionTime
                })
            );

                return studyCollection;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Ошибка при получении изображений: {ex.Message}");
                return new ObservableCollection<ImageLiteDb>();
            }
        }

        public ImageLiteDb Insert(ImageLiteDb image)
        {
            var entity = new LiteImage
            {
                Id = image.Id,
                SopInstanceUid = image.SopInstanceUid,
                SeriesInstanceUid = image.SeriesInstanceUid,
                StudyInstanceUid = image.StudyInstanceUid,
                BodyPart = image.BodyPart,
                Projection = image.Projection,
                Laterality = image.Laterality,
                InstanceDose = image.InstanceDose,
                AcquisitionTime = image.AcquisitionTime
            };

            _context.Images.Upsert(entity);

            image.Id = entity.Id;

            return image;
        }

        public void InsertBulk(IEnumerable<ImageLiteDb> images)
        {
            if (images == null || !images.Any())
                return;

            var entities = images.Select(image => new LiteImage
            {
                Id = image.Id,
                SopInstanceUid = image.SopInstanceUid,
                SeriesInstanceUid = image.SeriesInstanceUid,
                StudyInstanceUid = image.StudyInstanceUid,
                BodyPart = image.BodyPart,
                Projection = image.Projection,
                Laterality = image.Laterality,
                InstanceDose = image.InstanceDose,
                AcquisitionTime = image.AcquisitionTime
            }).ToList();

            _context.Images.InsertBulk(entities);
        }

        public void DeleteAll()
        {
            _context.Images.DeleteAll();
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

        public void Update(ImageLiteDb image)
        {
            throw new NotImplementedException();
        }

        public bool Any()
        {
            return _context.Images.Count() > 0;
        }
    }
}
