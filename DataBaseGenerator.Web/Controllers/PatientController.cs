using DataBaseGenerator.Core;
using DataBaseGenerator.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DataBaseGenerator.Web.Controllers
{
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;


        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public IActionResult Index()
        {
            var patient = _patientService.GetAll();

            if (!patient.Any())
                ViewBag.Message = "Пациенты в базе отсутствуют.";

            var viewmodel = patient.Select(p => new PatientViewModel
            {
                Id = p.ID_Patient,
                LastName = p.LastName,
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                BirthDate = p.BirthDate,
                Sex = p.Sex,
                Address = p.Address,
                AddInfo = p.AddInfo,
                Occupation = p.Occupation
            });

            return View(viewmodel);
        }

    }
}
