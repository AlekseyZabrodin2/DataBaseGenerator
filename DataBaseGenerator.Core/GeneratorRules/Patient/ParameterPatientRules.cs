using System;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class ParameterPatientNameRule : IGeneratorRule<string>
    {
        private readonly PatientGeneratorDto _options;
        private readonly RandomLastNameRule _lastName = new();
        private readonly RandomFirstNameRule _firstName = new();
        private readonly RandomMiddleNameRule _middleName = new();

        public ParameterPatientNameRule(PatientGeneratorDto options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string Generate()
        {
            return $"{_lastName.GenerateLastName(_options)}^{_firstName.GenerateFirstName(_options)}^{_middleName.GenerateMiddleNames(_options)}";
        }
    }

    public sealed class ParameterBirthDateRule : IGeneratorRule<DateTime?>
    {
        private readonly RandomBirthDateRule _birthDate = new(DateTime.Now);
        private readonly PatientGeneratorDto _options;

        public ParameterBirthDateRule(PatientGeneratorDto options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public DateTime? Generate()
        {
            return _birthDate.GenerateBirthdate(_options);
        }
    }

    public sealed class FixedBirthDateRule : IGeneratorRule<DateTime?>
    {
        private readonly DateTime _birthDate;

        public FixedBirthDateRule(DateTime birthDate)
        {
            _birthDate = birthDate;
        }

        public DateTime? Generate()
        {
            return _birthDate;
        }
    }
}
