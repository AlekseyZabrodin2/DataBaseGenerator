using System;

namespace DataBaseGenerator.Core
{
    public class WorkListGeneratorParameters
    {
        public WorkListGeneratorParameters(
            IGeneratorRule<int, int> iD_WorkList,
            IGeneratorRule<DateTime> createDate,
            IGeneratorRule<TimeSpan> createTime,
            IGeneratorRule<DateTime> completeDate,
            IGeneratorRule<TimeSpan> completeTime,
            IGeneratorRule<int, int> iD_Patient,
            IGeneratorRule<string> state,
            IGeneratorRule<string> sOPInstanceUID,
            IGeneratorRule<string> modality,
            IGeneratorRule<string> stationAeTitle,
            IGeneratorRule<DateTime> procedureStepStartDateTime,
            IGeneratorRule<string> performingPhysiciansName,
            IGeneratorRule<string> studyDescription,
            IGeneratorRule<string> referringPhysiciansName,
            IGeneratorRule<string> requestingPhysician
        )

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
            PerformingPhysiciansName =  performingPhysiciansName?? throw new ArgumentNullException(nameof(performingPhysiciansName));
            StudyDescription = studyDescription ?? throw new ArgumentNullException(nameof(studyDescription));
            ReferringPhysiciansName = referringPhysiciansName ?? throw new ArgumentNullException(nameof(referringPhysiciansName));
            RequestingPhysician = requestingPhysician ?? throw new ArgumentNullException(nameof(requestingPhysician));
        }


        public int WorkListCount { get; set; }

        public IGeneratorRule<int, int> ID_WorkList { get; }

        public IGeneratorRule<DateTime> CreateDate { get; }

        public IGeneratorRule<TimeSpan> CreateTime { get; }

        public IGeneratorRule<DateTime> CompleteDate { get; }

        public IGeneratorRule<TimeSpan> CompleteTime { get; }

        public IGeneratorRule<int, int> ID_Patient { get; }

        public IGeneratorRule<string> State { get; }

        public IGeneratorRule<string> SOPInstanceUID { get; }

        public IGeneratorRule<string> Modality { get; set; }

        public IGeneratorRule<string> StationAeTitle { get; }

        public IGeneratorRule<DateTime> ProcedureStepStartDateTime { get; }

        public IGeneratorRule<string> PerformingPhysiciansName { get; }

        public IGeneratorRule<string> StudyDescription { get; }

        public IGeneratorRule<string> ReferringPhysiciansName { get; }

        public IGeneratorRule<string> RequestingPhysician { get; }
    }
}