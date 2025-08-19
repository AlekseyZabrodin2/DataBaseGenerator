using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomBirthDateRule : IGeneratorRule<DateTime?>
    {
        private readonly Random _random = new();
        private readonly DateTime _nowDate = DateTime.Now;

        private int _randomMonth = 0;
        private int _randomDay = 0;

        public RandomBirthDateRule(DateTime birthDate)
        {
            BirthDate = birthDate;
        }

        public DateTime BirthDate { get; }

        private DateTime CreateDate(int year)
        {
            _randomMonth = _random.Next(1, 13);
            _randomDay = _random.Next(1, DateTime.DaysInMonth(year, _randomMonth) + 1);

            return new DateTime(year, _randomMonth, _randomDay);
        }

        public DateTime? Generate() => CreateDate(_random.Next(1950, _nowDate.Year));

        public DateTime? GenerateMissingBirthdate() => null;

        public DateTime? GenerateFutureBirthdate() => CreateDate(_random.Next(_nowDate.Year, _nowDate.Year + 100));

        public DateTime? GenerateAgeFrom0To17() => CreateDate(_random.Next(_nowDate.Year - 17, _nowDate.Year));

        public DateTime? GenerateAgeFrom18To60() => CreateDate(_random.Next(_nowDate.Year - 60, _nowDate.Year - 18));

        public DateTime? GenerateAgeFrom61To120() => CreateDate(_random.Next(_nowDate.Year - 120, _nowDate.Year - 61));


        public DateTime? GenerateBirthdate(PatientGeneratorDto patientGenerator)
        {
            var generatorBirthdate = new List<Func<DateTime?>>();

            if (patientGenerator.RandomBirthdateGeneratorRule)
                generatorBirthdate.Add(Generate);

            if (patientGenerator.MissingBirthdateGeneratorRule)
                generatorBirthdate.Add(GenerateMissingBirthdate);

            if (patientGenerator.FutureBirthdateGeneratorRule)
                generatorBirthdate.Add(GenerateFutureBirthdate);

            if (patientGenerator.Age0_17_GeneratorRule)
                generatorBirthdate.Add(GenerateAgeFrom0To17);

            if (patientGenerator.Age18_60_GeneratorRule)
                generatorBirthdate.Add(GenerateAgeFrom18To60);

            if (patientGenerator.Age61_120_GeneratorRule)
                generatorBirthdate.Add(GenerateAgeFrom61To120);

            if (generatorBirthdate.Count == 0)
                return _nowDate;

            var birthdate = generatorBirthdate[_random.Next(generatorBirthdate.Count)];
            var result = birthdate();

            return result;
        }

        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}
