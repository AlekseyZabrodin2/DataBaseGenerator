using System.Collections.Generic;

namespace DataBaseGenerator.Core
{
    public interface IPatientService
    {
        List<Patient> GetAll();
        void Generate(PatientGeneratorParameters inputParameters);
        void AddOne(PatientInputParameters inputParameters);
        string Create(int patientIndex, PatientGeneratorParameters patientGeneratorParameters);
        string CreateOne(PatientInputParameters patientGeneratorParameters);
        string DeleteFirst();
        string DeleteAll();
        string Edite(Patient oldPatient, int iD, string lastName, string name);
    }
}
