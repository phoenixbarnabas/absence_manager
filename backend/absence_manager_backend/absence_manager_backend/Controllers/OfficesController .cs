using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Collections.Generic;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/offices")]
    [Authorize]
    [RequiredScope("user_impersonation")]
    public class OfficesController : ControllerBase
    {
        private readonly OfficeManagementLogic _officeManagementLogic;

        public OfficesController(OfficeManagementLogic officeManagementLogic)
        {
            _officeManagementLogic = officeManagementLogic;
        }

        [HttpGet("by-location/{locationId}")]
        public IActionResult GetByLocation(string locationId, [FromQuery] bool activeOnly = false)
        {
            try
            {
                var result = _officeManagementLogic.GetOfficesByLocation(locationId, activeOnly);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}