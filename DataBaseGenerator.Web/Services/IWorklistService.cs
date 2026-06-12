using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Web.Services
{
    public interface IWorklistService
    {
        Task<List<WorkList>> GetAllAsync();
        Task<int> GetWorkListCountAsync();
        Task GenerateAsync(WorkListGeneratorDto inputParameters, CancellationToken cancellationToken);
        Task CreateAsync(WorkListGeneratorDto inputParameters, CancellationToken cancellationToken);
        Task DeleteFirstAsync();
        Task DeleteAllAsync();
    }
}
