using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.Core.MySqlGenerator.Data;
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

        public async Task<int> GetWorkListCountAsync()
        {
            try
            {
                return await _context.WorkList.CountAsync();
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "Can't get workList count");
                return 0;
            }
        }

        public async Task GenerateAsync(WorkListGeneratorDto inputParameters, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info("Cancellation requested, stopping generation...");
                    return;
                }

                await CreateAsync(inputParameters, cancellationToken);
                _logger.Info($"Created {inputParameters.WorkListCount} WorkLists");
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex, "WorkList not generated");
            }            
        }

        public async Task CreateAsync(WorkListGeneratorDto inputParameters, CancellationToken cancellationToken)
        {
            var total = inputParameters.WorkListCount;
            var batchSize = 1000;
            var newWorkLists = new List<WorkList>(batchSize);

            var maxId = await _context.WorkList
                .AnyAsync(cancellationToken)
                ? await _context.WorkList.MaxAsync(w => w.ID_WorkList, cancellationToken)
                : 0;

            var currentId = maxId;

            for (var workListIndex = 0; workListIndex < total; workListIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                currentId ++;

                var newWorkList = GenerateWorkList(currentId, inputParameters);
                newWorkLists.Add(newWorkList);

                if (newWorkLists.Count >= batchSize || workListIndex == total - 1)
                {
                    await _context.WorkList.AddRangeAsync(newWorkLists, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                    newWorkLists.Clear();
                }
            }
        }

        private WorkList GenerateWorkList(int workListIndex, WorkListGeneratorDto inputParameters)
        {
            return new WorkList()
            {
                WorkListID = workListIndex,
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
