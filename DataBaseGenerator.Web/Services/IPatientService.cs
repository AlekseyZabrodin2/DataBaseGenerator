using DataBaseGenerator.Core;

namespace DataBaseGenerator.Web.Services
{
    public interface IPatientService
    {
        Task<List<Patient>> GetAllAsync();
        Task GenerateAsync(PatientGeneratorDto inputParameters);
        Task AddOneAsync(PatientInputParameters inputParameters);
        Task CreateAsync(int patientIndex, PatientGeneratorDto patientGeneratorParameters);
        Task CreateOne(PatientInputParameters patientGeneratorParameters);
        Task DeleteFirstAsync();
        Task DeleteAllAsync();
        Task EditeAsync(Patient oldPatient, int iD, string lastName, string name);
        Task<bool> ConnectingEchoAsync();
    }
}
