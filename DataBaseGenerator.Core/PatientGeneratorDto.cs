using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.GeneratorRules.Patient;

namespace DataBaseGenerator.Core
{
    public class PatientGeneratorDto
    {
        public PatientGeneratorDto(

            OrderIdPatientRule iD_Patient,
            RandomLastNameRule lastName,
            RandomFirstNameRule firstName,
            RandomMiddleNameRule middleName,
            OrderPatientIdRule patientId,
            RandomBirthDateRule birthDate,
            RandomSexRule sex,
            RandomAddressRule address,
            RandomAddInfoRule addInfo,
            RandomOccupationRule occupation)
        {
            ID_Patient = iD_Patient ?? throw new ArgumentNullException(nameof(iD_Patient));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            MiddleName = middleName ?? throw new ArgumentNullException(nameof(middleName));
            PatientID = patientId ?? throw new ArgumentNullException(nameof(patientId));
            BirthDate = birthDate ?? throw new ArgumentNullException(nameof(birthDate));
            Sex = sex ?? throw new ArgumentNullException(nameof(sex));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            AddInfo = addInfo ?? throw new ArgumentNullException(nameof(addInfo));
            Occupation = occupation ?? throw new ArgumentNullException(nameof(occupation));
        }


        public int PatientCount { get; set; }

        public OrderIdPatientRule ID_Patient { get; }

        public RandomLastNameRule LastName { get; }

        public RandomFirstNameRule FirstName { get; }

        public RandomMiddleNameRule MiddleName { get; }

        public OrderPatientIdRule PatientID { get; }

        public RandomBirthDateRule BirthDate { get; }

        public RandomSexRule Sex { get; }

        public RandomAddressRule Address { get; }

        public RandomAddInfoRule AddInfo { get; }

        public RandomOccupationRule Occupation { get; }
    }
}
