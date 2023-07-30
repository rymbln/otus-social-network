using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OtusSocialNetwork.Services;

public interface IAuthenticatedUserService
{
    string UserId { get; }
}
public class AuthenticatedUserService : IAuthenticatedUserService
{
    public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
    {
        UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue("uid") ?? string.Empty;
        NameIdentifier = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
      
    }

    public string UserId { get; }
    public string NameIdentifier { get; }
   
}
