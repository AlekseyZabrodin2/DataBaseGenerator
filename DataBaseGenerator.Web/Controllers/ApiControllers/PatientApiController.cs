using System.Collections.ObjectModel;
using DataBaseGenerator.Core;
using DataBaseGenerator.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataBaseGenerator.Web.Controllers.ApiControllers
{

    [ApiController]
    [Route("api/patient")]
    public class PatientApiController : ControllerBase
    {
        private readonly IPatientService _patientService;


        public PatientApiController(IPatientService patientService)
        {
            _patientService = patientService;
        }         


        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var patients = await _patientService.GetAllAsync();
            return Ok(patients);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateAsync([FromBody] PatientGeneratorDto inputParameters)
        {
            await _patientService.GenerateAsync(inputParameters);
            return Ok("Patient added");
        }

        [HttpPost("addOne")]
        public async Task<IActionResult> AddOneAsync([FromBody] PatientInputParameters inputParameters)
        {
            await _patientService.AddOneAsync(inputParameters);
            return Ok("One patient added");
        }

        [HttpDelete("deleteFirst")]
        public async Task<IActionResult> DeleteFirstAsync()
        {
            await _patientService.DeleteFirstAsync();
            return Ok("First patient deleted");
        }

        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteAll()
        {
            await _patientService.DeleteAllAsync();
            return Ok("All patient deleted");
        }

        [HttpPost("edite")]
        public async Task<IActionResult> EditeAsync(ObservableCollection<Patient> patients)
        {
            await _patientService.EditeAsync(patients);
            return Ok("Patients Collection edited");
        }

        [HttpGet("echo")]
        public async Task<IActionResult> ConnectingEchoAsync()
        {
            await _patientService.ConnectingEchoAsync();
            return Ok("Echo success");
        }
    }
}
