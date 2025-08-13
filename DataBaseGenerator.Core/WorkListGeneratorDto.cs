using System;
using DataBaseGenerator.Core.GeneratorRules.WorkList;

namespace DataBaseGenerator.Core
{
    public class WorkListGeneratorDto
    {
        public WorkListGeneratorDto(
            OrderIdWorklistRule iD_WorkList,
            RandomCreateDateRule createDate,
            RandomCreateTimeRule createTime,
            RandomCompleteDateRule completeDate,
            RandomCompleteTimeRule completeTime,
            OrderIdPatientWlRule iD_Patient,
            RandomStateRule state,
            RandomSOPInstanceUIDRule sOPInstanceUID,
            RandomModalityRule modality,
            RandomStationAeTitleRule stationAeTitle,
            RandomProcedureStepStartDateTimeRule procedureStepStartDateTime,
            RandomPerformingPhysiciansNameRule performingPhysiciansName,
            RandomStudyDescriptionRule studyDescription,
            RandomReferringPhysiciansNameRule referringPhysiciansName,
            RandomRequestingPhysicianRule requestingPhysician)
        {
            ID_WorkList = iD_WorkList ?? throw new ArgumentNullException(nameof(iD_WorkList));
            CreateDate = createDate ?? throw new ArgumentNullException(nameof(createDate));
            CreateTime = createTime ?? throw new ArgumentNullException(nameof(createTime));
            CompleteDate = completeDate ?? throw new ArgumentNullException(nameof(completeDate));
            CompleteTime = completeTime ?? throw new ArgumentNullException(nameof(completeTime));
            ID_Patient = iD_Patient ?? throw new ArgumentNullException(nameof(iD_Patient));
            State = state ?? throw new ArgumentNullException(nameof(state));
            SOPInstanceUID = sOPInstanceUID ?? throw new ArgumentNullException(nameof(sOPInstanceUID));
            Modality = modality ?? throw new ArgumentNullException(nameof(modality));
            StationAeTitle = stationAeTitle ?? throw new ArgumentNullException(nameof(stationAeTitle));
            ProcedureStepStartDateTime = procedureStepStartDateTime ?? throw new ArgumentNullException(nameof(procedureStepStartDateTime));
            PerformingPhysiciansName = performingPhysiciansName ?? throw new ArgumentNullException(nameof(performingPhysiciansName));
            StudyDescription = studyDescription ?? throw new ArgumentNullException(nameof(studyDescription));
            ReferringPhysiciansName = referringPhysiciansName ?? throw new ArgumentNullException(nameof(referringPhysiciansName));
            RequestingPhysician = requestingPhysician ?? throw new ArgumentNullException(nameof(requestingPhysician));
        }


        public int WorkListCount { get; set; }

        public OrderIdWorklistRule ID_WorkList { get; }

        public RandomCreateDateRule CreateDate { get; }

        public RandomCreateTimeRule CreateTime { get; }

        public RandomCompleteDateRule CompleteDate { get; }

        public RandomCompleteTimeRule CompleteTime { get; }

        public OrderIdPatientWlRule ID_Patient { get; }

        public RandomStateRule State { get; }

        public RandomSOPInstanceUIDRule SOPInstanceUID { get; }

        public RandomModalityRule Modality { get; set; }

        public RandomStationAeTitleRule StationAeTitle { get; }

        public RandomProcedureStepStartDateTimeRule ProcedureStepStartDateTime { get; }

        public RandomPerformingPhysiciansNameRule PerformingPhysiciansName { get; }

        public RandomStudyDescriptionRule StudyDescription { get; }

        public RandomReferringPhysiciansNameRule ReferringPhysiciansName { get; }

        public RandomRequestingPhysicianRule RequestingPhysician { get; }

    }
}
