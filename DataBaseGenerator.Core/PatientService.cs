using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DataBaseGenerator.Core
{
    public class PatientService
    {
        private readonly HttpClient _httpClient;


        public PatientService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("DBGeneratorApi");
        }


        public async Task<List<Patient>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("patient/all");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Patient>>(content);
        }

        public async Task GenerateAsync(PatientGeneratorParameters inputParameters)
        {
            var json = JsonConvert.SerializeObject(inputParameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("patient/generate", content);
            response.EnsureSuccessStatusCode();
        }  

        public async Task AddOneAsync(PatientInputParameters inputParameters)
        {
            var json = JsonConvert.SerializeObject(inputParameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("patient/addOne", content);
            response.EnsureSuccessStatusCode();
        }        

        public async Task DeleteFirstAsync()
        {
            var response = await _httpClient.DeleteAsync("patient/deleteFirst");
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAllAsync()
        {
            var response = await _httpClient.DeleteAsync("patient/deleteAll");
            response.EnsureSuccessStatusCode();
        }

        public async Task EditeAsync(Patient oldPatient, int iD, string lastName, string name)
        {
            var inputParameters = $"{oldPatient}, {iD}, {lastName} {name}";
            var json = JsonConvert.SerializeObject(inputParameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("patient/edite", content);
        }
    }
}
