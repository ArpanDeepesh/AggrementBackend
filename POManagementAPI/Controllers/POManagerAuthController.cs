using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLogic;
using POManagementAPI.Services;
using POManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using POManagementAPI.Helper;
using BusinessLogic.DataModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace POManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class POManagerAuthController : ControllerBase
    {
        private readonly IPOManagerService _service;

        public POManagerAuthController(IPOManagerService service)
        {
            _service = service;
        }
        // GET: api/<POManagerController>
        [AuthAuthorize]
        [HttpGet("SendOtp")]
        public async Task<bool> NotifyUser(string phNo)
        {
            if (!phNo.Contains('+') && phNo.StartsWith(" 91"))
            {
                return await _service.NotifyUser("+" + phNo.Trim());
            }
            return false;

        }
        [AuthAuthorize]
        [HttpPost("login")]
        public async Task<LoginResponseModel> ValidateOTP([FromBody] OTPRequest request)
        {
            return await _service.login(request);
        }
        [POManagerAuthorize]
        [HttpGet("getClientInfo")]
        public async Task<GenericResponseModel<Client>> GetClient() {
            return new GenericResponseModel<Client>(getTokenData()) { Status=GeneralResponseStatus.SUCCESS,Message="User Data"};
        }
        [POManagerAuthorize]
        [HttpPost("UpdateClient")]
        public async Task<IActionResult> UpdateClient([FromBody] Client client)
        {
            var result = await _service.UpdateClientAsync(client);
            return Ok(result);
        }
        private Client getTokenData()
        {
            var tokenData = new Client();
            if (this.HttpContext.Items.ContainsKey("User"))
            {
                tokenData = this.HttpContext.Items["User"] as Client;
            }
            return tokenData;

        }

    }
}
