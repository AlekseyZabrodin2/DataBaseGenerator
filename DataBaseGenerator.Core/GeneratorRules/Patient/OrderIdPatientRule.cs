namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class OrderIdPatientRule : IGeneratorRule<int, int>
    {

        public int Generate(int parameter)
        {
            var iDPatient = parameter;

            return iDPatient;
        }
    }
}
