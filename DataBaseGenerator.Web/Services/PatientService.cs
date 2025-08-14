using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using Microsoft.EntityFrameworkCore;
using NLog;
using ILogger = NLog.ILogger;

namespace DataBaseGenerator.Web.Services
{
    public class PatientService : IPatientService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly BaseGenerateContext _context;
        private Patient _patient;


        public PatientService(BaseGenerateContext context)
        {
            _context = context;
        }


        private void LogAllExceptions(Exception ex, string message)
        {
            int level = 0;
            var current = ex;
            while (current != null)
            {
                _logger.Error(current, $"{message} (Level {level})");
                current = current.InnerException;
                level++;
            }
        }

        public async Task<List<Patient>> GetAllAsync()
        {
            try
            {
                var patients = await _context.Patient.ToListAsync();
                _logger.Info($"Loaded {patients.Count} patients");

                return patients;
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "Can`t get all patients");
                return new List<Patient>();
            }            
        }

        public async Task GenerateAsync(PatientGeneratorDto inputParameters)
        {
            try
            {
                for (var patientindex = 0; patientindex < inputParameters.PatientCount; patientindex++)
                {
                    await CreateAsync(patientindex, inputParameters);
                }
                _logger.Info($"Created {inputParameters.PatientCount} patients");
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "Patient not generated");
            }
        }

        public async Task CreateAsync(int patientIndex, PatientGeneratorDto patientGeneratorParameters)
        {
            _patient = GeneratePatients(patientIndex, patientGeneratorParameters);

            bool checkIsExist = _context.Patient.Any(element => element.ID_Patient == _patient.ID_Patient);

            if (checkIsExist)
                return;
            
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
            try
            {
                await CreateOne(inputParameters);
                _logger.Info($"Add patient");
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "One patient not generated");
            }            
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
            try
            {
                _context.Patient.Remove(_context.Patient.First());
                await _context.SaveChangesAsync();
                _logger.Info("Delete first patient");
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "First patient not deleted");
            }
        }

        public async Task DeleteAllAsync()
        {
            try
            {
                _context.Patient.RemoveRange(_context.Patient);
                await _context.SaveChangesAsync();
                _logger.Info("Delete all patients");
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "Patient table not deleted");
            }
        }

        public async Task EditeAsync(ObservableCollection<Patient> patients)
        {
            foreach (var patient in patients)
            {
                _context.Patient.Update(patient);
            }            

            await _context.SaveChangesAsync();
            _logger.Info("Edite patients collection");
        }

        public async Task<bool> ConnectingEchoAsync()
        {
            try
            {
                if (await _context.Database.CanConnectAsync())
                {
                    _logger.Info("Соединение с БД установлено");
                    return true;
                }
                else
                {
                    _logger.Warn("Не удалось подключиться к БД");
                    return false;
                }                
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при проверке соединения с БД");
                return false;
            }            
        }
    }
}
