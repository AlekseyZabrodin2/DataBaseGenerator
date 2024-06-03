using System;

namespace DataBaseGenerator.Core
{
    public class WorkList
    {
        public int WorkListID { get; set; }

        public int ID_WorkList { get; set; }

        public DateTime CreateDate { get; set; }
        
        public TimeSpan CreateTime { get; set; }
        
        public DateTime? CompleteDate { get; set; }

        public TimeSpan? CompleteTime { get; set; }

        public int ID_Patient { get; set; }

        public string State { get; set; }

        public string SOPInstanceUID { get; set; }

        public string Modality { get; set; }

        public string StationAeTitle { get; set; }

        public DateTime ProcedureStepStartDateTime { get; set; }

        public string PerformingPhysiciansName { get; set; }

        public string StudyDescription { get; set; }

        public string ReferringPhysiciansName { get; set; }

        public string RequestingPhysician { get; set; }

    }
}
