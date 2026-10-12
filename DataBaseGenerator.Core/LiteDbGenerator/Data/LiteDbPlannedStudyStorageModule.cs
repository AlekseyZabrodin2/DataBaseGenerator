using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.Repositories;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.Data
{
    public class LiteDbPlannedStudyStorageModule : IPlannedStudyStorageModule
    {
        private readonly LiteDbPlannedStudiesContext _database;

        public string Name => "LiteDB (Planned)";
        public string DatabaseKind => "Document";
        public string DatabasePath { get; set; }

        public IPlannedStudiesRepository PlannedStudies { get; set; }

        public LiteDbPlannedStudyStorageModule(string path, bool readOnly = false)
        {
            DatabasePath = path;

            if (!File.Exists(path))
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                using var created = new LiteDatabase(path);
            }

            var connectionString = new ConnectionString(path)
            {
                ReadOnly = readOnly,
            };

            _database = new LiteDbPlannedStudiesContext(connectionString);
            PlannedStudies = new LiteDbPlannedStudiesRepository(_database);
        }

        public List<string> GetCollectionNames() => _database.GetCollectionNames().ToList();

        public void Dispose() => _database?.Dispose();
    }
}
