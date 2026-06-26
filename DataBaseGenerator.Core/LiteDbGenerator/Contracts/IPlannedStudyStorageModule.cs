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

        IPlannedStudiesRepository PlannedStudies { get; }
    }
}
