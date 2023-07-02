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
      
    }

    public string UserId { get; }
   
}
