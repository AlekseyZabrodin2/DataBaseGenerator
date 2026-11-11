using System;
using System.Collections.Generic;
using System.IO;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.Core.GeneratorRules.Patient;
using System.Numerics;
using DataBaseGenerator.Core.GeneratorRules.WorkList;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using DataBaseGenerator.Core.LiteDbGenerator.PlannedStudy;
using FellowOakDicom;
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
                new RandomPhoneRule(),
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
            newPatient.Telephone.Generate();
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
                new RandomPhoneRule(),
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
            var patientIDs = new [] { 2, 15, 17 };

            var newWorkList = new WorkListGeneratorParameters(
                new OrderIdWorklistRule(),
                new RandomCreateDateRule(),
                new RandomCreateTimeRule(),
                new RandomCompleteDateRule(),
                new RandomCompleteTimeRule(),
                new OrderIdPatientWlRule(patientIDs),
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

            var result = workList.Generator(newWorkList, patientIDs);

            Console.WriteLine(result);
        }

        [TestMethod]
        public void OrderedIdRule_RandomPrefix_IsNewOnEachGenerate()
        {
            var rule = new OrderedIdRule("MXE", 0, 6, randomPrefix: true, prefixLength: 4);
            var id0 = rule.Generate(0);
            var id1 = rule.Generate(1);
            Assert.AreNotEqual(id0[..4], id1[..4], "Random prefix should change per Generate call.");
            StringAssert.StartsWith(id0, id0[..4]);
            Assert.AreEqual(10, id0.Length);
        }

        [TestMethod]
        public void PlannedStudyStubWrite_WritesDicomFiles()
        {
            var writer = new PlannedStudyDicomStubWriter();
            var study = new PlannedStudyLiteDb
            {
                Id = "2.25.118693752840759730111890450793372487131",
                StudyInstanceUid = "2.25.118693752840759730111890450793372487131",
                PatientId = "MXE0000005",
                PatientLastName = "Test",
                PatientFirstName = "Patient",
                StudyID = "STD-TEST",
                StudyDateTime = DateTime.Now.AddDays(-5),
                Gender = PatientSex.Other
            };
            var areas = new[]
            {
                new PlannedStudyArea
                {
                    BodyPartGuid = "075078f3-a940-492f-8874-67c989d1b36a",
                    AnatomicRegionCodeValue = "16982005",
                    ViewCodeValue = "399033003",
                    ProjectionUid = "256959603182452846723296224630387872385",
                    LateralityCode = "B"
                }
            };
            var details = new List<(string Description, string LateralityCode)> { ("Chest - Lung (frontal)", "U") };
            var tempRoot = Path.Combine(Path.GetTempPath(), "DicomStubTest_" + Guid.NewGuid().ToString("N"));
            var paths = writer.WriteStubs(tempRoot, study, areas, details);
            Assert.AreEqual(1, paths.Length);
            Assert.IsTrue(File.Exists(paths[0]), $"Missing file: {paths[0]}");

            var dataset = DicomFile.Open(paths[0]).Dataset;
            Assert.IsTrue(dataset.TryGetSequence(DicomTag.ViewCodeSequence, out var viewSeq));
            var viewItem = viewSeq.Items[0];
            Assert.AreEqual("399033003", viewItem.GetSingleValueOrDefault(DicomTag.CodeValue, string.Empty));
            Assert.AreEqual(
                "256959603182452846723296224630387872385",
                viewItem.GetSingleValueOrDefault(DicomTag.ContextUID, string.Empty));
            Assert.AreEqual("B", dataset.GetSingleValueOrDefault(DicomTag.ImageLaterality, string.Empty));

            var instanceDate = dataset.GetSingleValueOrDefault(DicomTag.InstanceCreationDate, DateTime.MinValue);
            var instanceTime = dataset.GetSingleValueOrDefault(DicomTag.InstanceCreationTime, DateTime.MinValue);
            var instanceCreated = instanceDate.Date.Add(instanceTime.TimeOfDay);
            Assert.IsTrue(instanceCreated >= study.StudyDateTime!.Value.AddSeconds(-1));
            Assert.IsTrue(instanceCreated <= DateTime.Now.AddSeconds(1));
        }
    }
}
