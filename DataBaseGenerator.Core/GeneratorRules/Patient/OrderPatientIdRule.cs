using System;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class OrderPatientIdRule : IGeneratorRule<string, int>
    {
        public string PatientID { get; set; } = "MXRZ-";

        public string Generate(int parameter)
        {
            if (string.IsNullOrWhiteSpace(PatientID))
            {
                throw new InvalidOperationException("Prefix must contain an string");
            }

            var patientId = PatientID + $"{parameter}".PadLeft(7,'0');

            return patientId;
        }
    }
}
