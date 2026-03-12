using System;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;
using LiteDB;

namespace DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels
{
    public sealed class LitePatient
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string PatientId { get; set; } = default!;

        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;

        public PatientSex Sex { get; set; } = PatientSex.Other;
        public DateTime? BirthDate { get; set; }

        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }
}
