using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace S3BucketwithAuth.Services;

public class TokenService : ITokenService
{
    private readonly IHttpContextAccessor _contextAccessor;
    public TokenService(IHttpContextAccessor httpContextAccessor)
    {
        _contextAccessor = httpContextAccessor;
    }
    public string GetUserDepartment()
    {
        var result = string.Empty;
        if(_contextAccessor.HttpContext is not null)
        {
            var claim = _contextAccessor.HttpContext.User.Claims.First(c => c.Type == "User_Department");
            result = claim.Value;
            //result = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            //result = _contextAccessor.HttpContext.User.FindFirst(JwtRegisteredClaimNames.Name).Value;
        }
        return result;
    }
}
