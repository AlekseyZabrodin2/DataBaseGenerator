using System.Collections.Generic;
using System.Linq;
using DataBaseGenerator.Core.Data;

namespace DataBaseGenerator.Core
{
    public class PatientService : IPatientService
    {
        private readonly BaseGenerateContext _context;

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
            var dataBaseGenerators = new List<PatientGeneratorParameters>();
            var options = _context;

            for (var patientindex = 0; patientindex < inputParameters.PatientCount; patientindex++)
            {
                var patients = Create(patientindex, inputParameters);

                dataBaseGenerators.Add(inputParameters);
            }
        }

        public void AddOne(PatientInputParameters inputParameters)
        {
            var dataBaseGenerators = new List<PatientInputParameters>();

            var patients = CreateOne(inputParameters);
            dataBaseGenerators.Add(inputParameters);
        }

        public string Create(int patientIndex, PatientGeneratorParameters patientGeneratorParameters)
        {
            string result = "Patient created";

            bool checkIsExist = _context.Patient.Any(element => element.ID_Patient == patientGeneratorParameters.ID_Patient.Generate(patientIndex));

            if (!checkIsExist)
            {
                Patient newPatient = new Patient
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

                _context.Patient.Add(newPatient);
                _context.SaveChanges();

                result = "Done";
            }

            return result;
        }

        public string CreateOne(PatientInputParameters patientGeneratorParameters)
        {
            string result = "Patient created";

            bool checkIsExist = _context.Patient.Any(element => element.PatientID == patientGeneratorParameters.PatientID);

            if (!checkIsExist)
            {
                Patient newPatient = new Patient
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

                _context.Patient.Add(newPatient);
                _context.SaveChanges();

                result = "Done";
            }

            return result;
        }

        public string DeleteFirst()
        {
            string result = "Patient is not create";

            _context.Patient.Remove(_context.Patient.First());
            _context.SaveChanges();

            result = $"Сделано! Пациент удален из базы";

            return result;
        }

        public string DeleteAll()
        {
            string result = "Patient is not create";

            _context.Patient.RemoveRange(_context.Patient);
            _context.SaveChanges();

            result = $"Сделано! Все Пациенты удалены из базы";

            return result;
        }        

        public string Edite(Patient oldPatient, int iD, string lastName, string name)
        {
            string result = "Patient is not create";

            Patient patient = _context.Patient.FirstOrDefault(position => position.ID_Patient == oldPatient.ID_Patient);
            if (patient != null)
            {
                patient.ID_Patient = iD;
                patient.LastName = lastName;
                patient.FirstName = name;
                _context.SaveChanges();

                result = $"Done!!! Patient data {patient.LastName} changed";
            }

            return result;
        }
    }
}
