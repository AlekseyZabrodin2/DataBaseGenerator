using FellowOakDicom;

namespace DataBaseGenerator.Core.GeneratorRules.Sop
{
    public interface ISopClassFactory
    {
        DicomDataset GenerateDataset(SopClassUid sopClassUid);
    }
}
