using Logic.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace Absence_Manager.Controllers
{
    [ApiController]
    [Route("api/locations")]
    [Authorize]
    [RequiredScope("user_impersonation")]
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
