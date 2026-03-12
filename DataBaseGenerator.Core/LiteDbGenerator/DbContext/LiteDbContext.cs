using System;
using DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbData.DbContext
{
    public sealed class LiteDbContext : IDisposable
    {
        private readonly LiteDatabase _database;

        public LiteDbContext(string path)
        {
            _database = new LiteDatabase(path);
        }

        public ILiteCollection<LitePatient> Patients =>
            _database.GetCollection<LitePatient>("patients");

        public ILiteCollection<LiteStudy> Studies =>
            _database.GetCollection<LiteStudy>("studies");

        public ILiteCollection<LiteSeries> Series =>
            _database.GetCollection<LiteSeries>("series");

        public ILiteCollection<LiteImage> Images =>
            _database.GetCollection<LiteImage>("images");

        public void Dispose() => _database.Dispose();
    }
}
