using System;

namespace DataBaseGenerator.Core.OrthancWorklists
{
    public sealed class OrthancWorklistPatientInput
    {
        public string PatientId { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public string FirstName { get; init; } = string.Empty;

        public string MiddleName { get; init; } = string.Empty;

        public DateTime? BirthDate { get; init; }

        public string Sex { get; init; } = string.Empty;

        public string Address { get; init; } = string.Empty;

        public string Telephone { get; init; } = string.Empty;

        public string Comments { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;
    }
}
