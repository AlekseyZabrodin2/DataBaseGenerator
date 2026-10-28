using DataBaseGenerator.Core.GeneratorRules.Sop;

namespace DataBaseGenerator.Core.DicomGenerator.DicomGeneratorModels
{
    public class SopGeneratorParameters
    {
        public SopGeneratorParameters(SopClassUidRule sopClassUidRule)
        {
            SopClassUidRule = sopClassUidRule;
        }

        public int SopCount { get; set; }

        public SopClassUidRule SopClassUidRule { get; }
    }
}
