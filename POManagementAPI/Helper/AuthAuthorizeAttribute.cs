using BusinessLogic;
using BusinessLogic.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace POManagementAPI.Helper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var client = context.HttpContext?.Items[ContractAPIConstants.ClientAuthRequest];
            if (client == null || client.ToString().ToLower() != ContractAPIConstants.ValidClientAuthRequest.ToLower())
            {
                // not logged in
                var message = "User is invalid" ;
                context.Result = new JsonResult(new { message = "Unauthorized:" + message }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
