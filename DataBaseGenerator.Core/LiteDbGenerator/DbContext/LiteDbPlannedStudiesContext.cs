using System;
using System.Collections.Generic;
using System.Linq;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.DbContext
{
    public sealed class LiteDbPlannedStudiesContext : IDisposable
    {
        private readonly LiteDatabase _database;

        public LiteDbPlannedStudiesContext(string path)
        {
            _database = new LiteDatabase(path);
        }

        public LiteDbPlannedStudiesContext(ConnectionString connectionString)
        {
            _database = new LiteDatabase(connectionString);
        }

        public List<string> GetCollectionNames()
        {
            return _database.GetCollectionNames().ToList();
        }

        public ILiteCollection<PlannedStudyLiteDb> PlannedStudies =>
            _database.GetCollection<PlannedStudyLiteDb>("planned_studies");

        public ILiteCollection<BsonDocument> PlannedStudiesDocuments =>
            _database.GetCollection<BsonDocument>("planned_studies");

        public void Dispose() => _database.Dispose();
    }
}
