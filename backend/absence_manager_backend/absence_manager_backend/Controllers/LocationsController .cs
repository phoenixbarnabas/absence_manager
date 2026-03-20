using Logic.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationsController : ControllerBase
    {
        private readonly OfficeManagementLogic _officeManagementLogic;

        public LocationsController(OfficeManagementLogic officeManagementLogic)
        {
            _officeManagementLogic = officeManagementLogic;
        }

        [HttpGet]
        public IActionResult GetLocations([FromQuery] bool activeOnly = false)
        {
            var result = _officeManagementLogic.GetLocations(activeOnly);
            return Ok(result);
        }
    }
}
