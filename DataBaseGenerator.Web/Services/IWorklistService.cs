using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Web.Services
{
    public interface IWorklistService
    {
        Task<List<WorkList>> GetAllAsync();
        Task GenerateAsync(WorkListGeneratorDto inputParameters);
        Task CreateAsync(int workListIndex, WorkListGeneratorDto inputParameters);
        Task DeleteFirstAsync();
        Task DeleteAllAsync();
    }
}
