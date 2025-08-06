using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseGenerator.Core
{
    public class PatientInputParameters
    {
        public PatientInputParameters(
            int iD_Patient,
            string lastName,
            string firstName,
            string middleName,
            string patientId,
            DateTime birthDate,
            string sex,
            string address,
            string addInfo,
            string occupation
            )
        {
            ID_Patient = iD_Patient ;
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            MiddleName = middleName ?? throw new ArgumentNullException(nameof(middleName));
            PatientID = patientId ?? throw new ArgumentNullException(nameof(patientId));
            BirthDate = birthDate;
            Sex = sex ?? throw new ArgumentNullException(nameof(sex));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            AddInfo = addInfo ?? throw new ArgumentNullException(nameof(addInfo));
            Occupation = occupation ?? throw new ArgumentNullException(nameof(occupation));
        }

        public int PatientCount { get; set; }

        public int ID_Patient { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string PatientID { get; set; }

        public DateTime BirthDate { get; set; }

        public string Sex { get; set; }

        public string Address { get; set; }

        public string AddInfo { get; set; }

        public string Occupation { get; set; }

    }
}
