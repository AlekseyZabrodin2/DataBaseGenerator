using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using Microsoft.EntityFrameworkCore;
using NLog;
using ILogger = NLog.ILogger;

namespace DataBaseGenerator.Web.Services
{
    public class WorklistService : IWorklistService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly BaseGenerateContext _context;


        public WorklistService(BaseGenerateContext context)
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

        public async Task<List<WorkList>> GetAllAsync()
        {
            try
            {
                var workList = await _context.WorkList.ToListAsync();
                _logger.Info($"Loaded {workList.Count} workList");

                return workList;
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "Can`t get all workLists");
                return new List<WorkList>();
            }
            
        }

        public async Task GenerateAsync(WorkListGeneratorDto inputParameters)
        {
            try
            {
                for (var workListIndex = 0; workListIndex < inputParameters.WorkListCount; workListIndex++)
                {
                    await CreateAsync(workListIndex, inputParameters);
                }
                _logger.Info($"Created {inputParameters.WorkListCount} WorkLists");
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "WorkList not generated");
            }
            
        }

        public async Task CreateAsync(int workListIndex, WorkListGeneratorDto inputParameters)
        {
            var newWorkList = GenerateWorkList(workListIndex, inputParameters);

            bool checkIsExist = _context.WorkList.Any(
                     workList => workList.ID_WorkList == newWorkList.ID_WorkList &&
                                 workList.CreateDate == newWorkList.CreateDate &&
                                 workList.CreateTime == newWorkList.CreateTime &&
                                 workList.ID_Patient == newWorkList.ID_Patient &&
                                 workList.State == newWorkList.State &&
                                 workList.SOPInstanceUID == newWorkList.SOPInstanceUID &&
                                 workList.Modality == newWorkList.Modality &&
                                 workList.StationAeTitle == newWorkList.StationAeTitle &&
                                 workList.ProcedureStepStartDateTime == newWorkList.ProcedureStepStartDateTime &&
                                 workList.PerformingPhysiciansName == newWorkList.PerformingPhysiciansName &&
                                 workList.StudyDescription == newWorkList.StudyDescription &&
                                 workList.ReferringPhysiciansName == newWorkList.ReferringPhysiciansName &&
                                 workList.RequestingPhysician == newWorkList.RequestingPhysician);

            if (checkIsExist)
                return;

            _context.WorkList.Add(newWorkList);
            await _context.SaveChangesAsync();
        }

        private WorkList GenerateWorkList(int workListIndex, WorkListGeneratorDto inputParameters)
        {
            return new WorkList()
            {
                ID_WorkList = inputParameters.ID_WorkList.Generate(workListIndex),
                CreateDate = inputParameters.CreateDate.Generate(),
                CreateTime = inputParameters.CreateTime.Generate(),
                ID_Patient = inputParameters.ID_Patient.Generate(workListIndex),
                State = inputParameters.State.Generate(),
                SOPInstanceUID = inputParameters.SOPInstanceUID.Generate(),
                Modality = inputParameters.Modality.Generate(),
                StationAeTitle = inputParameters.StationAeTitle.Generate(),
                ProcedureStepStartDateTime = inputParameters.ProcedureStepStartDateTime.Generate(),
                PerformingPhysiciansName = inputParameters.PerformingPhysiciansName.Generate(),
                StudyDescription = inputParameters.StudyDescription.Generate(),
                ReferringPhysiciansName = inputParameters.ReferringPhysiciansName.Generate(),
                RequestingPhysician = inputParameters.RequestingPhysician.Generate()
            };
        }

        public async Task DeleteFirstAsync()
        {
            try
            {
                _context.WorkList.Remove(_context.WorkList.First());
                await _context.SaveChangesAsync();

                _logger.Info("Delete First WorkList");
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "First workList not deleted");
            }
            
        }

        public async Task DeleteAllAsync()
        {
            try
            {
                _context.WorkList.RemoveRange(_context.WorkList);
                await _context.SaveChangesAsync();

                _logger.Info("Delete All WorkLists");
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "WorkList table not deleted");
            }
        }
    }
}
