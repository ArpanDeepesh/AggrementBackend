using Microsoft.Extensions.Options;
using BusinessLogic;
using POManagementAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace POManagementAPI.Helper
{
    public class JWTMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        public JWTMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }
        private Task BeginInvoke(HttpContext context)
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { (string)context.Request.Headers["Origin"] });
                context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept" });
                context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });
                context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });
                context.Response.StatusCode = 200;
                return context.Response.WriteAsync("OK");
            }

            return _next.Invoke(context);
        }
        public async Task Invoke(HttpContext context, IPOManagerService service)
        {
            var tokenParts = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ");
            var clientKey = context.Request.Headers["ClientKey"].FirstOrDefault();
            if (tokenParts != null && tokenParts.Count() == 2 && tokenParts.First() == "Bearer" && clientKey != null && validateClientKey(clientKey))
            {
                var token = tokenParts.Last();
                var key = _appSettings.AppSecret;
                if (token != null)
                {
                    attachUserToContext(context, service, token, key);
                }
            }
            else if (clientKey != null && validateClientKey(clientKey)) 
            {
                context.Items[ContractAPIConstants.ClientAuthRequest] = ContractAPIConstants.ValidClientAuthRequest;
            }
            await _next(context);
        }
        private bool validateClientKey(string clientKey)
        {
            return clientKey == _appSettings.ValidClientKey;
        }
        private void attachUserToContext(HttpContext context, IPOManagerService service, string token, string secretKey)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(secretKey);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var clientId = jwtToken.Claims.First(x => x.Type == ContractAPIConstants.AppClaimName).Value.ToString();

                // attach user to context on successful jwt validation
                context.Items["User"] = service.GetClientAsync(long.Parse(clientId)).Result;
            }
            catch (Exception ex)
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
