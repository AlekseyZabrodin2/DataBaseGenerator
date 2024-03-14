using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DataBaseGenerator.Core.Data;
using DataBaseGenerator.Core.GeneratorRules.Patient;
using Microsoft.EntityFrameworkCore;


namespace DataBaseGenerator.Core.Data
{
    public static class DataBaseCommand
    {
        public static List<Patient> GetAllPatients()
        {
            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                var patient = dataBase.Patient.ToList();
                return patient;
            }
        }


        public static List<WorkList> GetAllWorkLists()
        {
            using (BaseGenerateContext worklistBase = new BaseGenerateContext())
            {
                var worklist = worklistBase.WorkList.ToList();
                return worklist;
            }
        }


        public static IEnumerable<PatientGeneratorParameters> GeneratePatientDateBase(PatientGeneratorParameters patientGeneratorParameters)
        {
            var dataBaseGenerators = new List<PatientGeneratorParameters>();

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                for (var patientindex = 0; patientindex < patientGeneratorParameters.PatientCount; patientindex++)
                {
                    var patients = CreatePatient(patientindex + 1, patientGeneratorParameters);

                    dataBaseGenerators.Add(patientGeneratorParameters);
                }
            }

            return dataBaseGenerators;

        }


        public static IEnumerable<WorkListGeneratorParameters> GenerateWorkListBase(WorkListGeneratorParameters workListGeneratorParameters)
        {
            var workListGenerators = new List<WorkListGeneratorParameters>();

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                for (var workListIndex = 0; workListIndex < workListGeneratorParameters.WorkListCount; workListIndex++)
                {
                    var workList = CreateWorkList(workListIndex + 1, workListGeneratorParameters);

                    workListGenerators.Add(workListGeneratorParameters);
                }
            }

            return workListGenerators;

        }




        public static string CreatePatient(int patientIndex, PatientGeneratorParameters patientGeneratorParameters)
        {
            string result = "Patient created";

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                
                bool checkIsExist = dataBase.Patient.Any(
                    element =>element.ID_Patient == patientGeneratorParameters.ID_Patient.Generate(patientIndex) && element.LastName == patientGeneratorParameters.LastName.Generate() && element.FirstName == patientGeneratorParameters.FirstName.Generate()
                        && element.MiddleName == patientGeneratorParameters.MiddleName.Generate() && element.PatientID == patientGeneratorParameters.PatientID.Generate(patientIndex)
                        && element.BirthDate == patientGeneratorParameters.BirthDate.Generate() && element.Sex == patientGeneratorParameters.Sex.Generate()
                        && element.Address == patientGeneratorParameters.Address.Generate() && element.AddInfo == patientGeneratorParameters.AddInfo.Generate()
                        && element.Occupation == patientGeneratorParameters.Occupation.Generate());

                if (!checkIsExist)
                {
                    Patient newPatient = new Patient
                    {
                        ID_Patient = patientGeneratorParameters.ID_Patient.Generate(patientIndex),
                        LastName = patientGeneratorParameters.LastName.Generate(),
                        FirstName = patientGeneratorParameters.FirstName.Generate(),
                        MiddleName = patientGeneratorParameters.MiddleName.Generate(),
                        PatientID = patientGeneratorParameters.PatientID.Generate(patientIndex),
                        BirthDate = patientGeneratorParameters.BirthDate.Generate(),
                        Sex = patientGeneratorParameters.Sex.Generate(),
                        Address = patientGeneratorParameters.Address.Generate(),
                        AddInfo = patientGeneratorParameters.AddInfo.Generate(),
                        Occupation = patientGeneratorParameters.Occupation.Generate()
                    };

                    dataBase.Patient.Add(newPatient);
                    dataBase.SaveChanges();

                    result = "Done";

                }

                return result;
            }
        }



        public static string CreateWorkList(int workListIndex, WorkListGeneratorParameters workListGeneratorParameters)
        {
            string result = "WorkList created";

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {

                bool checkIsExist = dataBase.WorkList.Any(
                    workList => workList.ID_WorkList == workListGeneratorParameters.ID_WorkList.Generate(workListIndex) &&
                                workList.CreateDate == workListGeneratorParameters.CreateDate.Generate() &&
                                workList.CreateTime == workListGeneratorParameters.CreateTime.Generate() &&
                                //workList.CompleteDate == workListGeneratorParameters.CompleteDate.Generate() &&
                                //workList.CompleteTime == workListGeneratorParameters.CompleteTime.Generate() &&
                                workList.ID_Patient == workListGeneratorParameters.ID_Patient.Generate(workListIndex) && 
                                workList.State == workListGeneratorParameters.State.Generate() && 
                                workList.SOPInstanceUID == workListGeneratorParameters.SOPInstanceUID.Generate() && 
                                workList.Modality == workListGeneratorParameters.Modality.Generate() && 
                                workList.StationAeTitle == workListGeneratorParameters.StationAeTitle.Generate() && 
                                workList.ProcedureStepStartDateTime == workListGeneratorParameters.ProcedureStepStartDateTime.Generate() &&
                                workList.PerformingPhysiciansName == workListGeneratorParameters.PerformingPhysiciansName.Generate() && 
                                workList.StudyDescription == workListGeneratorParameters.StudyDescription.Generate() && 
                                workList.ReferringPhysiciansName == workListGeneratorParameters.ReferringPhysiciansName.Generate() && 
                                workList.RequestingPhysician == workListGeneratorParameters.RequestingPhysician.Generate()
                                );

                if (!checkIsExist)
                {
                    WorkList newWorkList = new WorkList()
                    {
                        ID_WorkList = workListGeneratorParameters.ID_WorkList.Generate(workListIndex),
                        CreateDate = workListGeneratorParameters.CreateDate.Generate(),
                        CreateTime = workListGeneratorParameters.CreateTime.Generate(),
                        //CompleteDate = workListGeneratorParameters.CompleteDate.Generate(),
                        //CompleteTime = workListGeneratorParameters.CompleteTime.Generate(),
                        ID_Patient = workListGeneratorParameters.ID_Patient.Generate(workListIndex),
                        State = workListGeneratorParameters.State.Generate(),
                        SOPInstanceUID = workListGeneratorParameters.SOPInstanceUID.Generate(),
                        Modality = workListGeneratorParameters.Modality.Generate(),
                        StationAeTitle = workListGeneratorParameters.StationAeTitle.Generate(),
                        ProcedureStepStartDateTime = workListGeneratorParameters.ProcedureStepStartDateTime.Generate(),
                        PerformingPhysiciansName = workListGeneratorParameters.PerformingPhysiciansName.Generate(),
                        StudyDescription = workListGeneratorParameters.StudyDescription.Generate(),
                        ReferringPhysiciansName = workListGeneratorParameters.ReferringPhysiciansName.Generate(),
                        RequestingPhysician = workListGeneratorParameters.RequestingPhysician.Generate()
                };

                    dataBase.WorkList.Add(newWorkList);
                    dataBase.SaveChanges();

                    result = "Done";

                }

                return result;
            }
        }


        public static string DeleteFirstPatient(PatientGeneratorParameters patientGeneratorParameters)
        {
            string result = "Patient is not create";

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                dataBase.Patient.Remove(dataBase.Patient.First());
                dataBase.SaveChanges();

                result = $"Сделано! Пациент удален из базы";

            }

            return result;
        }


        public static string DeleteAllPatients(PatientGeneratorParameters patientGeneratorParameters)
        {
            string result = "Patient is not create";

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                dataBase.Patient.RemoveRange(dataBase.Patient);
                dataBase.SaveChanges();

                result = $"Сделано! Все Пациенты удалены из базы";

            }

            return result;
        }


        public static string DeleteFirstWorkList(WorkListGeneratorParameters workListGeneratorParameters)
        {
            string result = "WorkList is not create";

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                dataBase.WorkList.Remove(dataBase.WorkList.First());
                dataBase.SaveChanges();

                result = $"Сделано! Первый из Рабочего списка удален";

            }

            return result;
        }


        public static string DeleteAllWorkList(WorkListGeneratorParameters workListGeneratorParameters)
        {
            string result = "WorkList is not create";

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                dataBase.WorkList.RemoveRange(dataBase.WorkList);
                dataBase.SaveChanges();

                result = $"Сделано! Весь рабочий список удален";

            }

            return result;
        }


        public static string EditePatient(Patient oldPatient, int iD, string lastName, string name)
        {
            string result = "Patient is not create";

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                Patient patient = dataBase.Patient.FirstOrDefault(position => position.ID_Patient == oldPatient.ID_Patient);
                if (patient != null)
                {
                    patient.ID_Patient = iD;
                    patient.LastName = lastName;
                    patient.FirstName = name;
                    dataBase.SaveChanges();

                    result = $"Done!!! Patient data {patient.LastName} changed";
                }
            }

            return result;
        }
    }
}
