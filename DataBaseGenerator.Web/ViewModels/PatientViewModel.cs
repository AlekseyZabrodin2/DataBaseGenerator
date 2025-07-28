namespace DataBaseGenerator.Web.ViewModels
{
    public class PatientViewModel
    {
        public int Id { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";

        public DateTime BirthDate { get; set; }

        public string BirthDateFormatted => BirthDate.ToString("dd.MM.yyyy");

        public string Sex { get; set; }

        public string Address { get; set; }

        public string AddInfo { get; set; }

        public string Occupation { get; set; }

    }
}
