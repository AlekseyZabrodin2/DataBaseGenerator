using System;

namespace DataBaseGenerator.Core.LiteDbGenerator.Models
{
    public class SeriesGeneratorDto
    {
        public string Id { get; set; }
        public string SeriesInstanceUid { get; set; }
        public string StudyInstanceUid { get; set; }
        public string OperatorName { get; set; }
        public string Modality { get; set; }
        public string BodyPartExamined { get; set; }
        public DateTime SeriesDateTime { get; set; }
    }
}
