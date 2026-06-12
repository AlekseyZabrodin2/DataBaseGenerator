using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Patient
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

            //var patientId = $"{PatientID}{parameter}";

            var patientId = PatientID + $"{parameter}".PadLeft(7,'0');

            return patientId;
        }

        //public override string ToString()
        //{
        //    return $"{Generate($"{PatientID}-{}")}";
        //}
    }
}
