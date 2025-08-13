using DataBaseGenerator.Core;
using DataBaseGenerator.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataBaseGenerator.Web.Controllers.ApiControllers
{

    [ApiController]
    [Route("api/worklist")]
    public class WorkListApiController : ControllerBase
    {
        private readonly IWorklistService _worklistService;

        public WorkListApiController(IWorklistService worklistService)
        {
            _worklistService = worklistService;
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync()        
        {
            var workList = await _worklistService.GetAllAsync();
            return Ok(workList);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateAsync([FromBody] WorkListGeneratorDto inputParameters)
        {
            await _worklistService.GenerateAsync(inputParameters);
            return Ok("WorkList added");
        }

        [HttpDelete("deleteFirst")]
        public async Task<IActionResult> DeleteFirstAsync()
        {
            await _worklistService.DeleteFirstAsync();
            return Ok("First in WorkList Delete");
        }

        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteAllAsync()
        {
            await _worklistService.DeleteAllAsync();
            return Ok("WorkList Table Deletion completed");
        }
    }
}
