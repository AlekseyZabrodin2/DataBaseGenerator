using System.Collections.Generic;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList;

namespace DataBaseGenerator.Core.MySqlGenerator
{
    public class WorkListGenerator
    {
        private string _modality;
        private string _aeTitle;

        public IEnumerable<WorkListGeneratorParameters> Generator(WorkListGeneratorParameters workListGeneratorParameters, IReadOnlyList<int> patientIds)
        {
            var workListGenerator = new List<WorkListGeneratorParameters>();

            for (int workListIndex = 0; workListIndex < workListGeneratorParameters.WorkListCount; workListIndex++)
            {
                var workLists = CreateWorkListModule(workListIndex, patientIds);

                workListGenerator.Add(workLists);
            }

            return workListGenerator;
        }

        public WorkListGeneratorParameters CreateWorkListModule(int workListIndex, IReadOnlyList<int> patientIds)
        {
            var newWorkList = new WorkListGeneratorParameters(
                new OrderIdWorklistRule(),
                new RandomCreateDateRule(),
                new RandomCreateTimeRule(),
                new RandomCompleteDateRule(),
                new RandomCompleteTimeRule(),
                new OrderIdPatientWlRule(patientIds),
                new RandomStateRule(),
                new RandomSOPInstanceUIDRule(),
                new RandomModalityRule(_modality),
                new RandomStationAeTitleRule(_aeTitle),
                new RandomProcedureStepStartDateTimeRule(),
                new RandomPerformingPhysiciansNameRule(),
                new RandomStudyDescriptionRule(),
                new RandomReferringPhysiciansNameRule(),
                new RandomRequestingPhysicianRule()
            );

            newWorkList.ID_WorkList.Generate(workListIndex);
            newWorkList.ID_Patient.Generate();
            newWorkList.CreateDate.Generate();
            newWorkList.CreateTime.Generate();
            //newWorkList.CompleteDate.Generate();
            //newWorkList.CompleteTime.Generate();
            newWorkList.State.Generate();
            newWorkList.SOPInstanceUID.Generate();
            newWorkList.Modality.Generate();
            newWorkList.StationAeTitle.Generate();
            newWorkList.ProcedureStepStartDateTime.Generate();
            newWorkList.PerformingPhysiciansName.Generate();
            newWorkList.StudyDescription.Generate();
            newWorkList.ReferringPhysiciansName.Generate();
            newWorkList.RequestingPhysician.Generate();

            return newWorkList;
        }
    }
}
