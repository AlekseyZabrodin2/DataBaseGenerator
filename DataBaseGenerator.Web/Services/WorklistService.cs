using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace DataBaseGenerator.Web.Services
{
    public class WorklistService : IWorklistService
    {
        private readonly BaseGenerateContext _context;
        private WorkList _newWorkList;
        private DbContextOptions<BaseGenerateContext> _options = new DbContextOptionsBuilder<BaseGenerateContext>()
            .UseMySql("server = localhost; database = medxregistry; user = root; password = root; port = 3306"
            , new MySqlServerVersion(new Version(8, 0, 28))).Options;


        public WorklistService(BaseGenerateContext context)
        {
            _context = context;
        }


        public async Task<List<WorkList>> GetAllAsync()
        {
            return await _context.WorkList.ToListAsync();
        }

        public async Task GenerateAsync(WorkListGeneratorDto inputParameters)
        {
            for (var workListIndex = 0; workListIndex < inputParameters.WorkListCount; workListIndex++)
            {
                using var context = new BaseGenerateContext(_options);
                await CreateAsync(workListIndex, inputParameters, context);
            }
        }

        public async Task CreateAsync(int workListIndex, WorkListGeneratorDto inputParameters, BaseGenerateContext context)
        {
            _newWorkList = GenerateWorkList(workListIndex, inputParameters);

            bool checkIsExist = context.WorkList.Any(
                     workList => workList.ID_WorkList == inputParameters.ID_WorkList.Generate(workListIndex) &&
                                 workList.CreateDate == inputParameters.CreateDate.Generate() &&
                                 workList.CreateTime == inputParameters.CreateTime.Generate() &&
                                 workList.ID_Patient == inputParameters.ID_Patient.Generate(workListIndex) &&
                                 workList.State == inputParameters.State.Generate() &&
                                 workList.SOPInstanceUID == inputParameters.SOPInstanceUID.Generate() &&
                                 workList.Modality == inputParameters.Modality.Generate() &&
                                 workList.StationAeTitle == inputParameters.StationAeTitle.Generate() &&
                                 workList.ProcedureStepStartDateTime == inputParameters.ProcedureStepStartDateTime.Generate() &&
                                 workList.PerformingPhysiciansName == inputParameters.PerformingPhysiciansName.Generate() &&
                                 workList.StudyDescription == inputParameters.StudyDescription.Generate() &&
                                 workList.ReferringPhysiciansName == inputParameters.ReferringPhysiciansName.Generate() &&
                                 workList.RequestingPhysician == inputParameters.RequestingPhysician.Generate());

            if (checkIsExist)
                return;

            context.WorkList.Add(_newWorkList);
            await context.SaveChangesAsync();
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
            _context.WorkList.Remove(_context.WorkList.First());
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync()
        {
            _context.WorkList.RemoveRange(_context.WorkList);
            await _context.SaveChangesAsync();
        }
    }
}
