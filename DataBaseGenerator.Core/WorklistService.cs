using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace DataBaseGenerator.Core
{
    public class WorklistService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly HttpClient _httpClient;


        public WorklistService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("DBGeneratorApi");
        }


        public async Task<List<WorkList>> GetAllAsync()
        {
            _logger.Trace("Get All worklists");

            var response = await _httpClient.GetAsync("worklist/all");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<WorkList>>(content);
        }

        public async Task GenerateAsync(WorkListGeneratorDto inputParameters)
        {
            _logger.Trace("Generate worklists");

            var json = JsonConvert.SerializeObject(inputParameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("worklist/generate", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteFirstAsync()
        {
            _logger.Trace("Delete First worklist");

            var response = await _httpClient.DeleteAsync("worklist/deleteFirst");
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAllAsync()
        {
            _logger.Trace("Delete All worklists");

            var response = await _httpClient.DeleteAsync("worklist/deleteAll");
            response.EnsureSuccessStatusCode();
        }
    }
}
