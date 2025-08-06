using System.Threading.Tasks;
using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace DataBaseGenerator.Web.Services
{
    public class PatientService : IPatientService
    {
        private readonly BaseGenerateContext _context;
        private Patient _patient;
        private DbContextOptions<BaseGenerateContext> _options = new DbContextOptionsBuilder<BaseGenerateContext>()
            .UseMySql("server = localhost; database = medxregistry; user = root; password = root; port = 3306"
            , new MySqlServerVersion(new Version(8, 0, 28))).Options;


        public PatientService(BaseGenerateContext context)
        {
            _context = context;
        }


        public async Task<List<Patient>> GetAllAsync()
        {
            return await _context.Patient.ToListAsync();
        }

        public async Task GenerateAsync(PatientGeneratorDto inputParameters)
        {
            for (var patientindex = 0; patientindex < inputParameters.PatientCount; patientindex++)
            {
               await CreateAsync(patientindex, inputParameters);
            }
        }

        public async Task CreateAsync(int patientIndex, PatientGeneratorDto patientGeneratorParameters)
        {
            bool checkIsExist = _context.Patient.Any(element => element.ID_Patient == patientGeneratorParameters.ID_Patient.Generate(patientIndex));

            if (checkIsExist)
                return;

            _patient = GeneratePatients(patientIndex, patientGeneratorParameters);
            _context.Patient.Add(_patient);
            await _context.SaveChangesAsync();
        }

        private Patient GeneratePatients(int patientIndex, PatientGeneratorDto patientGeneratorParameters)
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

        public async Task AddOneAsync(PatientInputParameters inputParameters)
        {
            await CreateOne(inputParameters);
        }

        public async Task CreateOne(PatientInputParameters patientGeneratorParameters)
        {
            bool checkIsExist = _context.Patient.Any(element => element.PatientID == patientGeneratorParameters.PatientID);

            if (checkIsExist)
                return;

            _patient = CreateOnePatient(patientGeneratorParameters);
            _context.Patient.Add(_patient);
            await _context.SaveChangesAsync();
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

        public async Task DeleteFirstAsync()
        {
            _context.Patient.Remove(_context.Patient.First());
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync()
        {
            _context.Patient.RemoveRange(_context.Patient);
            await _context.SaveChangesAsync();
        }

        public async Task EditeAsync(Patient oldPatient, int iD, string lastName, string name)
        {
            Patient patient = _context.Patient.FirstOrDefault(position => position.ID_Patient == oldPatient.ID_Patient);
            if (patient != null)
            {
                patient.ID_Patient = iD;
                patient.LastName = lastName;
                patient.FirstName = name;
                await _context.SaveChangesAsync();
            }
        }
    }
}
