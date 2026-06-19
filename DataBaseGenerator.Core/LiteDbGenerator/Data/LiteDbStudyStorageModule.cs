using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataBaseGenerator.Core.LiteDbData.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Repositories;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.Data
{
    public sealed class LiteDbStudyStorageModule : IStudyStorageModule
    {
        private readonly LiteDbContext _context;

        public LiteDbStudyStorageModule(string databasePath, bool readOnly)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException("Database path must be provided", nameof(databasePath));

            DatabasePath = databasePath;
            var connectionString = new ConnectionString
            {
                Filename = databasePath,
                ReadOnly = readOnly
            };

            _context = new LiteDbContext(connectionString);
            // В ReadOnly нельзя создавать индексы — это запись; из‑за этого часто сыпятся ошибки с -log.db.
            if (!readOnly)
            {
                IndexInitializer.EnsureIndexes(_context);
            }

            Patients = new LitePatientRepository(_context);
            Studies = new LiteStudyRepository(_context);
            Series = new LiteSeriesRepository(_context);
            Images = new LiteImageRepository(_context);
        }

        public string DatabasePath { get; }

        public string Name => "LiteDB";

        public string DatabaseKind => "Embedded";

        public IPatientRepository Patients { get; }

        public IStudyRepository Studies { get; }

        public ISeriesRepository Series { get; }

        public IImageRepository Images { get; }

        public List<string> GetCollectionNames()
        {
            return _context.GetCollectionNames().ToList();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
