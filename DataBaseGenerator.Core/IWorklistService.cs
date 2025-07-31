using System.Collections.Generic;
using DataBaseGenerator.Core.Data;

namespace DataBaseGenerator.Core
{
    public interface IWorklistService
    {
        List<WorkList> GetAll();
        void Generate(WorkListGeneratorParameters inputParameters);
        void Create(int workListIndex, WorkListGeneratorParameters inputParameters, BaseGenerateContext context);
        void DeleteFirst();
        void DeleteAll();
    }
}
