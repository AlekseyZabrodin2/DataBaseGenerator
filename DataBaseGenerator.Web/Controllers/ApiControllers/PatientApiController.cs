using System.Collections.ObjectModel;
using DataBaseGenerator.Core.MySqlGenerator;
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
        public async Task<IActionResult> GenerateAsync([FromBody] PatientGeneratorDto inputParameters, CancellationToken cancellationToken)
        {
            await _patientService.GenerateAsync(inputParameters, cancellationToken);
            return Ok("Patient added");
        }

        [HttpPost("addOne")]
        public async Task<IActionResult> AddOneAsync([FromBody] PatientInputParameters inputParameters)
        {
            var added = await _patientService.AddOneAsync(inputParameters);
            if (!added)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Content = "Пациент с таким ID_Patient уже существует",
                    ContentType = "text/plain"
                };
            }

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
