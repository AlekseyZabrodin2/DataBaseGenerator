using DataBaseGenerator.Core;
using DataBaseGenerator.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DataBaseGenerator.Web.Controllers
{
    public class WorkListController : Controller
    {
        private readonly IWorklistService _worklistService;


        public WorkListController(IWorklistService worklistService)
        {
            _worklistService = worklistService;
        }


        public IActionResult Index()
        {
            var worklist = _worklistService.GetAll();

            if (!worklist.Any())
                ViewBag.Message = "Рабочий список пуст";

            var viewmodel = worklist.Select(wl => new WorklistViewModel
            {
                ID_WorkList = wl.ID_WorkList,
                CreateDate = wl.CreateDate,
                CreateTime = wl.CreateTime,
                ID_Patient = wl.ID_Patient,
                State = wl.State,
                SOPInstanceUID = wl.SOPInstanceUID,
                Modality = wl.Modality,
                StationAeTitle = wl.StationAeTitle,
                ProcedureStepStartDateTime = wl.ProcedureStepStartDateTime,
                PerformingPhysiciansName = wl.PerformingPhysiciansName,
                StudyDescription = wl.StudyDescription,
                ReferringPhysiciansName = wl.ReferringPhysiciansName,
                RequestingPhysician = wl.RequestingPhysician
            });

            return View(viewmodel);
        }
    }
}
