using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.PlannedStudy
{
    public sealed class RandomProjectionPathsRule : IGeneratorRule<string[]>
    {
        private static readonly Random _random = new Random();

        private string GenerateRandomNumber(int length)
        {
            var chars = "0123456789";
            var stringChars = new char[length];
            var random = new Random();
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        private string GetRandomLetter()
        {
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return letters[_random.Next(letters.Length)].ToString();
        }

        public string[] Generate()
        {
            var count = _random.Next(1, 4); // от 1 до 3 путей
            var paths = new List<string>();

            for (int i = 0; i < count; i++)
            {
                var fileName = $"DX-{GetRandomLetter()}-{i + 1}-{GenerateRandomNumber(36)}.dcm";
                var path = $@"c:\DicomTempFolder\{fileName}";
                paths.Add(path);
            }

            return paths.ToArray();
        }
    }
}
