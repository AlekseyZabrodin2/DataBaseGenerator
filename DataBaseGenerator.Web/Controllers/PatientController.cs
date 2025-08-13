using DataBaseGenerator.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DataBaseGenerator.Web.Controllers
{
    public class PatientController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly List<PatientViewModel> _patientViewModels = new();

        public PatientController(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("DBGeneratorApi");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("patient/all");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Message = $"Проблемы с подключением к БД. - [{response.StatusCode}]";
                    return View(_patientViewModels);
                }

                var content = await response.Content.ReadAsStringAsync();
                var patients = JsonConvert.DeserializeObject<List<PatientViewModel>>(content);

                if (!patients.Any())
                    ViewBag.Message = "Пациенты в базе отсутствуют.";

                return View(patients);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Message = $"Ошибка подключения к серверу: {ex.Message}";
                return View(_patientViewModels);
            }
        }

    }
}
