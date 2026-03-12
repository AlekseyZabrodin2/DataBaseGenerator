using System;

namespace DataBaseGenerator.Core.LiteDbGenerator.Contracts
{
    public interface IStudyStorageModule : IDisposable
    {
        // Human-readable name of the storage implementation (e.g. "LiteDB", "SqlServer")
        string Name { get; }

        // Kind of database used (e.g. "Embedded", "Relational", "Document")
        string DatabaseKind { get; }

        IPatientRepository Patients { get; }
        IStudyRepository Studies { get; }
        ISeriesRepository Series { get; }
        IImageRepository Images { get; }
    }
}
