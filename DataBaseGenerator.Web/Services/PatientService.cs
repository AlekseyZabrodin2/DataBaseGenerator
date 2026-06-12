using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.Core.MySqlGenerator.Data;
using LiteDB;
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

        public async Task GenerateAsync(PatientGeneratorDto inputParameters, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Trace("Generate patients");

                if(cancellationToken.IsCancellationRequested)
                    {
                    _logger.Info("Cancellation requested, stopping generation...");
                    return;
                }

                await CreateByBulkAsync(inputParameters, cancellationToken);

                _logger.Info($"Created {inputParameters.PatientCount} patients");
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "Patient not generated");
                throw;
            }
        }

        public async Task CreateByOneAsync(PatientGeneratorDto patientGeneratorParameters, CancellationToken cancellationToken)
        {
            var total = patientGeneratorParameters.PatientCount;
            var batchSize = 1000;
            var patients = new List<Patient>(batchSize);
            var prefix = "MEX";
            var digitsCount = 7;
            var format = new string('0', digitsCount);

            var nextPatientId = GenerateNextPatientId(prefix, digitsCount, 1);
            var nonDigitPattern = new Regex(@"[^\d]");
            var numericPart = nonDigitPattern.Replace(nextPatientId, string.Empty);
            var currentNumber = uint.Parse(numericPart);

            for (var patientIndex = 0; patientIndex < total; patientIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info("Cancellation requested, stopping generation...");
                    break;
                }

                var newNumber = currentNumber + patientIndex;
                var patientId = $"{prefix}{newNumber.ToString(format)}";
                var patient = GeneratePatients((int)newNumber, patientGeneratorParameters, patientId);

                var exists = await _context.Patient.AnyAsync(p => p.ID_Patient == patient.ID_Patient, cancellationToken);
                if (exists)
                {
                    _logger.Warn($"Patient with ID {patient.ID_Patient} already exists, skipping...");
                    continue;
                }

                _context.Patient.Add(patient);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        ///  CREATE patients by 1000 and save, it`s faster 
        /// </summary>        
        public async Task CreateByBulkAsync(PatientGeneratorDto patientGeneratorParameters, CancellationToken cancellationToken)
        {
            var total = patientGeneratorParameters.PatientCount;
            var batchSize = 1000;
            var patients = new List<Patient>(batchSize);
            var prefix = "MEX";
            var digitsCount = 7;
            var format = new string('0', digitsCount);
            
            var existingIds = await _context.Patient
                .Where(p => p.PatientID.StartsWith(prefix))
                .Select(p => p.PatientID)
                .ToListAsync(cancellationToken);

            var nonDigitPattern = new Regex(@"[^\d]");

            var maxNumber = existingIds.Any()
                ? existingIds
                    .Select(id =>
                    {
                        var numPart = nonDigitPattern.Replace(id, string.Empty);
                        return uint.TryParse(numPart, out var num) ? num : 0;
                    })
                    .Max()
                : 0;

            var currentNumber = maxNumber;
            _logger.Info($"Starting from number: {currentNumber + 1}");

            for (var patientIndex = 0; patientIndex < total; patientIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var newNumber = currentNumber + patientIndex + 1;
                var patientId = $"{prefix}{newNumber.ToString(format)}";
                var patient = GeneratePatients((int)newNumber, patientGeneratorParameters, patientId);

                patients.Add(patient);

                if (patients.Count >= batchSize || patientIndex == total - 1)
                {
                    await _context.Patient.AddRangeAsync(patients, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                    patients.Clear();
                }
            }

            _logger.Info($"Created {total} patients. Last ID: {prefix}{(currentNumber + total).ToString(format)}");
        }

        private Patient GeneratePatients(int patientIndex, PatientGeneratorDto patientGeneratorParameters, string newPatientId)
        {
            return new Patient
            {
                ID_Patient = patientGeneratorParameters.ID_Patient.Generate(patientIndex),
                LastName = patientGeneratorParameters.LastName.GenerateLastName(patientGeneratorParameters),
                FirstName = patientGeneratorParameters.FirstName.GenerateFirstName(patientGeneratorParameters),
                MiddleName = patientGeneratorParameters.MiddleName.GenerateMiddleNames(patientGeneratorParameters),
                PatientID = newPatientId, //patientGeneratorParameters.PatientID.Generate(patientIndex),
                BirthDate = patientGeneratorParameters.BirthDate.GenerateBirthdate(patientGeneratorParameters),
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

        public string GenerateNextPatientId(string prefix, int digitsCount, int startsFrom = 1)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(digitsCount);

            var format = new string('0', digitsCount);

            var matchedIds = _context.Patient
                .Select(p => p.PatientID)
                .Where(id => id.StartsWith(prefix))
                .ToList();

            if (matchedIds.Count == 0)
                return $"{prefix}{((uint)startsFrom).ToString(format)}";

            var nonDigitPattern = new Regex(@"[^\d]");

            var maxIdValue = matchedIds
                .Select(id =>
                {
                    var integerPart = nonDigitPattern.Replace(id, string.Empty);
                    return uint.TryParse(integerPart, out var result) ? result : 0;
                })
                .Max();

            var newIdValue = maxIdValue + 1 < (uint)startsFrom
                ? (uint)startsFrom
                : maxIdValue + 1;

            return $"{prefix}{newIdValue.ToString(format)}";
        }
    }
}
