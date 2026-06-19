using System;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Image;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Study;

namespace DataBaseGenerator.Core.LiteDbGenerator.Models
{
    public class ImageGeneratorDto
    {
        public ImageGeneratorDto(
            RandomProjectionRule projection,
            RandomLateralityRule aterality)
        {
            RandomProjection = projection ?? throw new ArgumentNullException(nameof(projection));
            RandomLaterality = aterality ?? throw new ArgumentNullException(nameof(aterality));
        }

        public string Id { get; set; }
        public string SopInstanceUid { get; set; }
        public string SeriesInstanceUid { get; set; }
        public string StudyInstanceUid { get; set; }
        public string BodyPart { get; set; }
        public string Projection { get; set; }
        public string Laterality { get; set; }
        public double InstanceDose { get; set; }
        public DateTime AcquisitionTime { get; set; }

        public RandomProjectionRule RandomProjection { get; }
        public RandomLateralityRule RandomLaterality { get; }
    }
}
