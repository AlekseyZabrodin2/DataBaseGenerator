using System.Collections.Generic;

namespace DataBaseGenerator.Core
{
    public interface IWorklistService
    {
        List<WorkList> GetAll();
        void Generate(WorkListGeneratorParameters inputParameters);
        string Create(int workListIndex, WorkListGeneratorParameters inputParameters);
        string DeleteFirst();
        string DeleteAll();
    }
}
