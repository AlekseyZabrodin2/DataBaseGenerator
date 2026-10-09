using System.Collections.ObjectModel;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Web.Services
{
    public interface IPatientService
    {
        Task<List<Patient>> GetAllAsync();
        Task GenerateAsync(PatientGeneratorDto inputParameters, CancellationToken cancellationToken);
        Task<bool> AddOneAsync(PatientInputParameters inputParameters);
        Task CreateByOneAsync(PatientGeneratorDto patientGeneratorParameters, CancellationToken cancellationToken); 
        Task CreateByBulkAsync(PatientGeneratorDto patientGeneratorParameters, CancellationToken cancellationToken);
        Task<bool> CreateOne(PatientInputParameters patientGeneratorParameters);
        Task DeleteFirstAsync();
        Task DeleteAllAsync();
        Task EditeAsync(ObservableCollection<Patient> patients);
        Task<bool> ConnectingEchoAsync();
    }
}
