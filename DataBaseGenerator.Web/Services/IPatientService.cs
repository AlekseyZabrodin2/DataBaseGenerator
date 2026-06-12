using System.Collections.ObjectModel;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Web.Services
{
    public interface IPatientService
    {
        Task<List<Patient>> GetAllAsync();
        Task GenerateAsync(PatientGeneratorDto inputParameters, CancellationToken cancellationToken);
        Task AddOneAsync(PatientInputParameters inputParameters);
        Task CreateByOneAsync(PatientGeneratorDto patientGeneratorParameters, CancellationToken cancellationToken); 
        Task CreateByBulkAsync(PatientGeneratorDto patientGeneratorParameters, CancellationToken cancellationToken);
        Task CreateOne(PatientInputParameters patientGeneratorParameters);
        Task DeleteFirstAsync();
        Task DeleteAllAsync();
        Task EditeAsync(ObservableCollection<Patient> patients);
        Task<bool> ConnectingEchoAsync();
    }
}
