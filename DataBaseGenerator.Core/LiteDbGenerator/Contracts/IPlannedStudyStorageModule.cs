using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.LiteDbGenerator.Contracts
{
    public interface IPlannedStudyStorageModule : IDisposable
    {
        string Name { get; }
        string DatabaseKind { get; }
        string DatabasePath { get; }

        List<string> GetCollectionNames();

        IReadOnlyList<IReadOnlyDictionary<string, string>> ReadCollection(string collectionName);

        IPlannedStudiesRepository PlannedStudies { get; }
    }
}
