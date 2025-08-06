using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace DataBaseGenerator.Core
{
    public class WorklistService
    {
        private readonly HttpClient _httpClient;


        public WorklistService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("DBGeneratorApi");
        }


        public async Task<List<WorkList>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("worklist/all");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<WorkList>>(content);
        }

        public async Task GenerateAsync(WorkListGeneratorDto inputParameters)
        {
            var json = JsonConvert.SerializeObject(inputParameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("worklist/generate", content);
            response.EnsureSuccessStatusCode();

            //var content = await response.Content.ReadAsStringAsync();
            //return JsonConvert.DeserializeObject<List<WorkList>>(content);
        }

        public async Task DeleteFirstAsync()
        {
            var response = await _httpClient.DeleteAsync("worklist/deleteFirst");
            response.EnsureSuccessStatusCode();

            //var content = await response.Content.ReadAsStringAsync();
            //return JsonConvert.DeserializeObject<List<WorkList>>(content);
        }

        public async Task DeleteAllAsync()
        {
            var response = await _httpClient.DeleteAsync("worklist/deleteAll");
            response.EnsureSuccessStatusCode();

            //var content = await response.Content.ReadAsStringAsync();
            //return JsonConvert.DeserializeObject<List<WorkList>>(content);
        }
    }
}
