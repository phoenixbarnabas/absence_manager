using Logic.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/offices")]
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
            var result = _officeManagementLogic.GetOfficesByLocation(locationId, activeOnly);
            return Ok(result);
        }
    }
}
