using System.Collections.Generic;
using System.Linq;
using DataBaseGenerator.Core.Data;

namespace DataBaseGenerator.Core
{
    public class PatientService : IPatientService
    {
        private readonly BaseGenerateContext _context;
        private Patient _patient;


        public PatientService(BaseGenerateContext context)
        {
            _context = context;
        }


        public List<Patient> GetAll()
        {
            return _context.Patient.ToList();
        }

        public void Generate(PatientGeneratorParameters inputParameters)
        {
            for (var patientindex = 0; patientindex < inputParameters.PatientCount; patientindex++)
            {
                Create(patientindex, inputParameters);
            }
        }        

        public void Create(int patientIndex, PatientGeneratorParameters patientGeneratorParameters)
        {
            bool checkIsExist = _context.Patient.Any(element => element.ID_Patient == patientGeneratorParameters.ID_Patient.Generate(patientIndex));

            if (checkIsExist)
                return;

            _patient = GeneratePatients(patientIndex, patientGeneratorParameters);
            _context.Patient.Add(_patient);
            _context.SaveChanges();
        }

        private Patient GeneratePatients(int patientIndex, PatientGeneratorParameters patientGeneratorParameters)
        {
            return new Patient
            {
                ID_Patient = patientGeneratorParameters.ID_Patient.Generate(patientIndex),
                LastName = patientGeneratorParameters.LastName.Generate(),
                FirstName = patientGeneratorParameters.FirstName.Generate(),
                MiddleName = patientGeneratorParameters.MiddleName.Generate(),
                PatientID = patientGeneratorParameters.PatientID.Generate(patientIndex),
                BirthDate = patientGeneratorParameters.BirthDate.Generate(),
                Sex = patientGeneratorParameters.Sex.Generate(),
                Address = patientGeneratorParameters.Address.Generate(),
                AddInfo = patientGeneratorParameters.AddInfo.Generate(),
                Occupation = patientGeneratorParameters.Occupation.Generate()
            };
        }

        public void AddOne(PatientInputParameters inputParameters)
        {
            CreateOne(inputParameters);
        }

        public void CreateOne(PatientInputParameters patientGeneratorParameters)
        {
            bool checkIsExist = _context.Patient.Any(element => element.PatientID == patientGeneratorParameters.PatientID);

            if (checkIsExist)
                return;

            _patient = CreateOnePatient(patientGeneratorParameters);
            _context.Patient.Add(_patient);
            _context.SaveChanges();
        }

        private Patient CreateOnePatient(PatientInputParameters patientGeneratorParameters)
        {
            return new Patient
            {
                ID_Patient = patientGeneratorParameters.ID_Patient,
                LastName = patientGeneratorParameters.LastName,
                FirstName = patientGeneratorParameters.FirstName,
                MiddleName = patientGeneratorParameters.MiddleName,
                PatientID = patientGeneratorParameters.PatientID,
                BirthDate = patientGeneratorParameters.BirthDate,
                Sex = patientGeneratorParameters.Sex,
                Address = patientGeneratorParameters.Address,
                AddInfo = patientGeneratorParameters.AddInfo,
                Occupation = patientGeneratorParameters.Occupation
            };
        }

        public void DeleteFirst()
        {
            _context.Patient.Remove(_context.Patient.First());
            _context.SaveChanges();
        }

        public void DeleteAll()
        {
            _context.Patient.RemoveRange(_context.Patient);
            _context.SaveChanges();
        }        

        public void Edite(Patient oldPatient, int iD, string lastName, string name)
        {
            Patient patient = _context.Patient.FirstOrDefault(position => position.ID_Patient == oldPatient.ID_Patient);
            if (patient != null)
            {
                patient.ID_Patient = iD;
                patient.LastName = lastName;
                patient.FirstName = name;
                _context.SaveChanges();
            }
        }
    }
}
