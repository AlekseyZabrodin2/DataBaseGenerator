using System;
using System.Collections.Generic;
using System.Linq;
using DataBaseGenerator.Core.LiteDbGenerator.Data;
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

        public LiteDbContext(ConnectionString connectionString)
        {
            _database = new LiteDatabase(connectionString);
        }

        public List<string> GetCollectionNames()
        {
            return _database.GetCollectionNames().ToList();
        }

        public IReadOnlyList<IReadOnlyDictionary<string, string>> ReadCollection(string collectionName)
        {
            return LiteDbDocumentReader.Read(_database.GetCollection(collectionName).FindAll());
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
