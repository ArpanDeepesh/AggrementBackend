using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("Connect")]
        public async Task<string> Connect()
        {
            return "You are able to connect with PO Manager backend API.";
        }
    }
}
