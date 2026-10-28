namespace DataBaseGenerator.Core.GeneratorRules.Series
{
    public class SeriesNumberRule : IGeneratorRule<string, int>
    {
        public string Generate(int number)
        {
            return $"{number}";
        }
    }
}
