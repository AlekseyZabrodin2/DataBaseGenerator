using System;
using System.Linq;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;

namespace DataBaseGenerator.Core.LiteDbGenerator.Models
{
    public sealed class PlannedStudyLiteDb
    {
        public string Id { get; set; }
        public string PatientLastName { get; set; }
        public string PatientFirstName { get; set; }
        public string PatientMiddleName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }
        public string Age { get; set; }
        public string Comments { get; set; }
        public PatientSex Gender { get; set; }
        public string PatientId { get; set; }
        public string StudyID { get; set; }
        public string AccessionNumber { get; set; }
        public string PatientIdDataBase { get; set; }
        public string StudyInstanceUid { get; set; }
        public PlannedStudyArea[] StudyAreas { get; set; }
        public string[] ProjectionPaths { get; set; }
        public PlannedStudyStatus Status { get; set; }
        public int? ImagesCount { get; set; }
        public string PatientFullName
        {
            get
            {
                // Join only non-empty parts to avoid double spaces when middle name is missing.
                return string.Join(" ", new[] { PatientLastName, PatientFirstName, PatientMiddleName }
                    .Where(part => !string.IsNullOrWhiteSpace(part)));
            }
        }
    }
}
