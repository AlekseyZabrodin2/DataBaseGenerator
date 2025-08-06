using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using Microsoft.AspNetCore.Mvc;

namespace DataBaseGenerator.Web.Services
{
    public interface IWorklistService
    {
        Task<List<WorkList>> GetAllAsync();
        Task GenerateAsync(WorkListGeneratorDto inputParameters);
        Task CreateAsync(int workListIndex, WorkListGeneratorDto inputParameters, BaseGenerateContext context);
        Task DeleteFirstAsync();
        Task DeleteAllAsync();
    }
}
