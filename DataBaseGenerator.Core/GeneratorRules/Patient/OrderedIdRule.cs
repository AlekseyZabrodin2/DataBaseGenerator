using System;
using System.Numerics;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class OrderedIdRule : IGeneratorRule<string, int>
    {
        public string Prefix { get; set; }
        public BigInteger IdNumber { get; set; }
        public int IdLength { get; set; }
        public bool UseFixedId { get; set; }
        public string FixedPatientId { get; set; }
        private readonly bool _useRandomPrefix;
        private readonly int _prefixLength;


        public OrderedIdRule(string prefix, BigInteger idNumber, int idLength = 6, bool randomPrefix = false, int prefixLength = 4, bool useFixedId = false, string fixedPatientId = null)
        {
            IdNumber = idNumber;
            IdLength = Math.Max(1, idLength);
            UseFixedId = useFixedId;
            FixedPatientId = fixedPatientId;
            _useRandomPrefix = randomPrefix;
            _prefixLength = Math.Clamp(prefixLength, 1, 64);

            Prefix = _useRandomPrefix
                ? string.Empty
                : string.IsNullOrEmpty(prefix) ? "Test_" : prefix;
        }

        public string Generate(int order)
        {
            if (UseFixedId && !string.IsNullOrWhiteSpace(FixedPatientId))
                return FixedPatientId.Trim();

            if (order < 0)
                order = 0;

            var prefix = _useRandomPrefix
                ? RandomLetters(_prefixLength)
                : Prefix;

            if (string.IsNullOrWhiteSpace(prefix))
                throw new InvalidOperationException("Prefix must contain a string.");

            var entered = !string.IsNullOrWhiteSpace(FixedPatientId);
            var idNumber = entered ? IdNumber + order : IdNumber + order + 1;
            var patientId = $"{prefix}{idNumber.ToString("D" + IdLength)}";

            if (patientId.Length > 64)
                throw new InvalidOperationException("Generated PatientID exceeds the maximum DICOM length of 64 characters.");

            return patientId;
        }

        private static string RandomLetters(int length)
        {
            const int MaxPatientIdLength = 64;
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            length = Math.Clamp(length, 0, MaxPatientIdLength);

            var letters = new char[length];

            for (var index = 0; index < length; index++)
            {
                letters[index] = alphabet[Random.Shared.Next(alphabet.Length)];
            }

            return new string(letters);
        }
    }
}
