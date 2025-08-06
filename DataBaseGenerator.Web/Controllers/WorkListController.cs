using System.Threading.Tasks;
using DataBaseGenerator.Web.Services;
using DataBaseGenerator.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DataBaseGenerator.Web.Controllers
{
    public class WorkListController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly List<WorklistViewModel> _worklistViewModels = new();

        public WorkListController(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("DBGeneratorApi");
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("worklist/all");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Message = $"Проблемы с подключением к БД. - [{response.StatusCode}]";
                    return View(_worklistViewModels);
                }

                var content = await response.Content.ReadAsStringAsync();
                var worklist = JsonConvert.DeserializeObject<List<WorklistViewModel>>(content) ?? new();

                if (!worklist.Any())
                    ViewBag.Message = "Рабочий список пуст";                

                return View(worklist);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Message = $"Ошибка подключения к серверу: {ex.Message}";
                return View(_worklistViewModels);
            }            
        }
    }
}
