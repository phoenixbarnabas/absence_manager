using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/workstations")]
    [Authorize]
    [RequiredScope("user_impersonation")]
    public class WorkstationsController : ControllerBase
    {
        private readonly OfficeManagementLogic _officeManagementLogic;

        public WorkstationsController(OfficeManagementLogic officeManagementLogic)
        {
            _officeManagementLogic = officeManagementLogic;
        }

        [HttpGet("by-office/{officeId}")]
        public IActionResult GetByOffice(string officeId, [FromQuery] bool activeOnly = false)
        {
            try { 
                var result = _officeManagementLogic.GetWorkstationsByOffice(officeId, activeOnly);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{workstationId}")]
        public IActionResult GetById(string workstationId)
        {
            try
            {
                var result = _officeManagementLogic.GetWorkstationById(workstationId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
