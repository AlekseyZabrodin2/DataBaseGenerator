using System;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class OrderedIdRule : IGeneratorRule<string, int>
    {
        public string Prefix { get; set; }
        public int IdNumber { get; set; }
        public int IdLength { get; set; }


        public OrderedIdRule(string prefix, int idNumber, int idLength = 6, bool randomPrefix = false, int prefixLength = 4)
        {
            IdNumber = idNumber;
            IdLength = idLength < 1 ? 1 : idLength;
            Prefix = randomPrefix
                ? RandomLetters(prefixLength < 1 ? 1 : prefixLength)
                : string.IsNullOrEmpty(prefix) ? "Test_" : prefix;
        }

        public string Generate(int order)
        {
            if (order < 0)
            {
                order = 0;
            }

            var idNumber = IdNumber + order + 1;

            if (string.IsNullOrWhiteSpace(Prefix))
            {
                throw new InvalidOperationException("Prefix must contain an string");
            }

            return $"{Prefix}{idNumber.ToString("D" + IdLength)}";
        }

        private static string RandomLetters(int length)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            var letters = new char[length];

            for (var index = 0; index < length; index++)
            {
                letters[index] = alphabet[random.Next(alphabet.Length)];
            }

            return new string(letters);
        }
    }
}
