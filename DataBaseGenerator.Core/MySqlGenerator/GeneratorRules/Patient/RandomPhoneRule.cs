using System;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Patient
{
    public sealed class RandomPhoneRule : IGeneratorRule<string>
    {
        private static readonly string[] _operatorCodes =
        [
            "25",
            "29",
            "33",
            "44"
        ];

        public string Generate()
        {
            var random = new Random();

            var operatorCode = _operatorCodes[random.Next(_operatorCodes.Length)];
            var number = random.Next(1000000, 10000000);

            return $"+375 ({operatorCode}) {number:D7}";
        }

        public override string ToString()
        {
            return Generate();
        }
    }
}
