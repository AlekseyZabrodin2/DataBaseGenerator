﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.GeneratorRules.WorkList;

namespace DataBaseGenerator.Core
{
    public class WorkListGenerator
    {
        public IEnumerable<WorkListGeneratorParameters> Generator(WorkListGeneratorParameters workListGeneratorParameters)
        {
            var workListGenerator = new List<WorkListGeneratorParameters>();

            for (int workListIndex = 0; workListIndex < workListGeneratorParameters.WorkListCount; workListIndex++)
            {
                var workLists = CreateWorkListModule(workListIndex);

                workListGenerator.Add(workLists);
            }

            return workListGenerator;
        }

        public WorkListGeneratorParameters CreateWorkListModule(int workListIndex)
        {
            var newWorkList = new WorkListGeneratorParameters(
                new OrderIdWorklistRule(),
                new RandomCreateDateRule(),
                new RandomCreateTimeRule(),
                new RandomCompleteDateRule(),
                new RandomCompleteTimeRule(),
                new OrderIdPatientWlRule(),
                new RandomStateRule(),
                new RandomSOPInstanceUIDRule(),
                new RandomModalityRule(),
                new RandomStationAeTitleRule(),
                new RandomProcedureStepStartDateTimeRule(),
                new RandomPerformingPhysiciansNameRule(),
                new RandomStudyDescriptionRule(),
                new RandomReferringPhysiciansNameRule(),
                new RandomRequestingPhysicianRule()
            );

            newWorkList.ID_WorkList.Generate(workListIndex);
            newWorkList.CreateDate.Generate();
            newWorkList.CreateTime.Generate();
            //newWorkList.CompleteDate.Generate();
            //newWorkList.CompleteTime.Generate();
            newWorkList.ID_Patient.Generate(workListIndex);
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
