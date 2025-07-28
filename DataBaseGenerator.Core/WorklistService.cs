using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataBaseGenerator.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace DataBaseGenerator.Core
{
    public class WorklistService : IWorklistService
    {
        private readonly BaseGenerateContext _context;

        public WorklistService(BaseGenerateContext context)
        {
            _context = context;
        }


        public List<WorkList> GetAll()
        {
            return _context.WorkList.ToList();
        }

        public void Generate(WorkListGeneratorParameters inputParameters)
        {
            var workListGenerators = new List<WorkListGeneratorParameters>();

            for (var workListIndex = 0; workListIndex < inputParameters.WorkListCount; workListIndex++)
            {
                var workList = Create(workListIndex, inputParameters);

                workListGenerators.Add(inputParameters);
            }

            //return workListGenerators;
        }

        public string Create(int workListIndex, WorkListGeneratorParameters inputParameters)
        {
            string result = "WorkList created";

            bool checkIsExist = _context.WorkList.Any(
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
                                 workList.RequestingPhysician == inputParameters.RequestingPhysician.Generate()
                                 );

            if (!checkIsExist)
            {
                WorkList newWorkList = new WorkList()
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

                _context.WorkList.Add(newWorkList);
                _context.SaveChanges();

                result = "Done";
            }

            return result;
        }       

        public string DeleteFirst()
        {
            string result = "WorkList is not create";

            _context.WorkList.Remove(_context.WorkList.First());
            _context.SaveChanges();

            result = $"Сделано! Первый из Рабочего списка удален";

            return result;
        }

        public string DeleteAll()
        {
            string result = "WorkList is not create";

            _context.WorkList.RemoveRange(_context.WorkList);
            _context.SaveChanges();

            result = $"Сделано! Весь рабочий список удален";

            return result;
        }
    }
}
