﻿using System;
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
                var result = dataBase.Patient.ToList();
                return result;
            }
        }


        public static string CreatePatient(string patientId, string lastName, string firstName, string middleName,
            DateTime birthDate, string sex, string address, string addInfo, string occupation)
        {
            string result = "Patient created";

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                bool checkIsExist = dataBase.Patient.Any(
                    element => element.LastName == lastName && element.FirstName == firstName && element.MiddleName == middleName
                    && element.PatientID == patientId && element.BirthDate == birthDate && element.Sex == sex && element.Address == address && element.AddInfo == addInfo);

                if (!checkIsExist)
                {
                    Patient newPatient = new Patient
                    {

                        LastName = lastName,
                        FirstName = firstName,
                        MiddleName = middleName,
                        PatientID = patientId,
                        BirthDate = birthDate,
                        Sex = sex,
                        Address = address,
                        AddInfo = addInfo,
                        Occupation = occupation
                    };

                    dataBase.Patient.Add(newPatient);
                    dataBase.SaveChanges();

                    result = "Done";

                }

                return result;
            }
        }

        
        public static string DeletePatient(Patient patient)
        {
            string result = "Patient is not create";

            using (BaseGenerateContext dataBase = new BaseGenerateContext())
            {
                dataBase.Patient.Remove(patient);
                dataBase.SaveChanges();

                result = $"Сделано! Пацоент {patient.LastName} удален из базы";

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
