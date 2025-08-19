using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataBaseGenerator.Core
{
    public class Patient
    {
        public int ID_Patient { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string PatientID { get; set; }

        public DateTime? BirthDate { get; set; }

        public string Sex { get; set; }

        public string Address { get; set; }

        public string AddInfo { get; set; }

        public string Occupation { get; set; }
        
    }
}