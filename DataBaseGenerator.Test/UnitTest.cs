using System;
using DataBaseGenerator.Core;
using DataBaseGenerator.Core.GeneratorRules.Patient;
using DataBaseGenerator.Core.GeneratorRules.WorkList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataBaseGenerator.Test
{
    [TestClass]
    public class UnitTest
    {
        private readonly string _modality;
        private readonly string _aeTitle;

        [TestMethod]
        public void RandomNameGenerate()
        {
            var name = new RandomFirstNameRule();
            var result = name.Generate();

            Console.WriteLine(result);
        }

        [TestMethod]
        public void RandomLastNameGenerate()
        {
            var lastName = new RandomLastNameRule();
            var result = lastName.Generate();

            Console.WriteLine(result);
        }

        [TestMethod]
        public void CreateNewPatient()
        {
            var newPatient = new PatientGeneratorParameters(
                new OrderIdPatientRule(),
                new RandomFirstNameRule(),
                new RandomLastNameRule(),
                new RandomMiddleNameRule(),
                new OrderPatientIdRule(),
                new RandomBirthDateRule(new DateTime()),
                new RandomSexRule(),
                new RandomAddressRule(),
                new RandomAddInfoRule(),
                new RandomOccupationRule()
            );

            newPatient.ID_Patient.Generate(1);
            newPatient.LastName.Generate();
            newPatient.FirstName.Generate();
            newPatient.MiddleName.Generate();
            newPatient.PatientID.Generate(1);
            newPatient.BirthDate.Generate();
            newPatient.Sex.Generate();
            newPatient.Address.Generate();
            newPatient.AddInfo.Generate();
            newPatient.Occupation.Generate();

            var result = newPatient;

            Console.WriteLine(result);
        }

        [TestMethod]
        public void GeneratePatient()
        {
            var patient = new PatientGenerator();

            var newPatient = new PatientGeneratorParameters(
                new OrderIdPatientRule(),
                new RandomFirstNameRule(),
                new RandomLastNameRule(),
                new RandomMiddleNameRule(),
                new OrderPatientIdRule(),
                new RandomBirthDateRule(new DateTime()),
                new RandomSexRule(),
                new RandomAddressRule(),
                new RandomAddInfoRule(),
                new RandomOccupationRule())
            {
                PatientCount = 4
            };
            


            var result = patient.Generate(newPatient);

            Console.WriteLine(result);
        }

        [TestMethod]
        public void CreateNewWorkList()
        {
            var workList = new WorkListGenerator();

            var newWorkList = new WorkListGeneratorParameters(
                new OrderIdWorklistRule(),
                new RandomCreateDateRule(),
                new RandomCreateTimeRule(),
                new RandomCompleteDateRule(),
                new RandomCompleteTimeRule(),
                new OrderIdPatientWlRule(),
                new RandomStateRule(),
                new RandomSOPInstanceUIDRule(),
                new RandomModalityRule(_modality),
                new RandomStationAeTitleRule(_aeTitle),
                new RandomProcedureStepStartDateTimeRule(),
                new RandomPerformingPhysiciansNameRule(),
                new RandomStudyDescriptionRule(),
                new RandomReferringPhysiciansNameRule(),
                new RandomRequestingPhysicianRule())
            {
                WorkListCount = 4
            };

            var result = workList.Generator(newWorkList);

            Console.WriteLine(result);
        }
    }
}
