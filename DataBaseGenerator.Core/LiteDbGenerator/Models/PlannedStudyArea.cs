using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.LiteDbGenerator.Models
{
    public sealed class PlannedStudyArea
    {
        public string BodyPartGuid { get; set; }

        public string AnatomicRegionCodeValue { get; set; }

        public string ViewCodeValue { get; set; }

        public string ProjectionUid { get; set; }

        public string LateralityCode { get; set; }

        public bool IsValid()
        {
            var hasRegion = !string.IsNullOrWhiteSpace(AnatomicRegionCodeValue);
            var hasView = !string.IsNullOrWhiteSpace(ProjectionUid)
                || !string.IsNullOrWhiteSpace(ViewCodeValue);

            return hasRegion && hasView;
        }

        public PlannedStudyArea Normalize()
        {
            return new PlannedStudyArea
            {
                BodyPartGuid = NormalizePart(BodyPartGuid),
                AnatomicRegionCodeValue = NormalizePart(AnatomicRegionCodeValue),
                ViewCodeValue = NormalizePart(ViewCodeValue),
                ProjectionUid = NormalizePart(ProjectionUid),
                LateralityCode = NormalizePart(LateralityCode)
            };
        }        

        public override string ToString()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(BodyPartGuid))
                parts.Add($"BodyPartGuid: {BodyPartGuid}");

            if (!string.IsNullOrWhiteSpace(AnatomicRegionCodeValue))
                parts.Add($"AnatomicRegionCodeValue: {AnatomicRegionCodeValue}");

            if (!string.IsNullOrWhiteSpace(ViewCodeValue))
                parts.Add($"ViewCodeValue: {ViewCodeValue}");

            if (!string.IsNullOrWhiteSpace(ProjectionUid))
                parts.Add($"ProjectionUid: {ProjectionUid}");

            if (!string.IsNullOrWhiteSpace(LateralityCode))
                parts.Add($"LateralityCode: {LateralityCode}");

            return string.Join(Environment.NewLine, parts);
        }

        private static string NormalizePart(string part)
        {
            return string.IsNullOrWhiteSpace(part)
                ? null
                : part.Trim();
        }
    }
}
