using System;
using DataBaseGenerator.Core.LiteDbData.DbContext;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Repositories;

namespace DataBaseGenerator.Core.LiteDbGenerator.Data
{
    public sealed class LiteDbStudyStorageModule : IStudyStorageModule
    {
        private readonly LiteDbContext _context;

        public LiteDbStudyStorageModule(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException("Database path must be provided", nameof(databasePath));

            _context = new LiteDbContext(databasePath);
            IndexInitializer.EnsureIndexes(_context);

            Patients = new LitePatientRepository(_context);
            Studies = new LiteStudyRepository(_context);
            Series = new LiteSeriesRepository(_context);
            Images = new LiteImageRepository(_context);
        }

        public string Name => "LiteDB";

        public string DatabaseKind => "Embedded";

        public IPatientRepository Patients { get; }

        public IStudyRepository Studies { get; }

        public ISeriesRepository Series { get; }

        public IImageRepository Images { get; }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
