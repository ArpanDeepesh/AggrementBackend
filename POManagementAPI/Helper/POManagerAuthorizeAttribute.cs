using BusinessLogic.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace POManagementAPI.Helper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class POManagerAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Items["User"] as Client;
            if (user == null)
            {
                // not logged in
                var message = "User is invalid" ;
                context.Result = new JsonResult(new { message = "Unauthorized:" + message }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
