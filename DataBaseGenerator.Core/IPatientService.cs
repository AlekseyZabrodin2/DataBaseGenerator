using System.Collections.Generic;

namespace DataBaseGenerator.Core
{
    public interface IPatientService
    {
        List<Patient> GetAll();
        void Generate(PatientGeneratorParameters inputParameters);
        void AddOne(PatientInputParameters inputParameters);
        void Create(int patientIndex, PatientGeneratorParameters patientGeneratorParameters);
        void CreateOne(PatientInputParameters patientGeneratorParameters);
        void DeleteFirst();
        void DeleteAll();
        void Edite(Patient oldPatient, int iD, string lastName, string name);
    }
}
