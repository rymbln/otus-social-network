using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OtusSocialNetwork.Services;

public interface IAuthenticatedUserService
{
    string UserId { get; }
    string Role { get; }
    string Name { get; }
    string Email { get; }
}
public class AuthenticatedUserService : IAuthenticatedUserService
{
    public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
    {
        UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue("uid") ?? string.Empty;
        Role = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        Name = httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Name) ?? string.Empty;
        Email = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        Claims = httpContextAccessor.HttpContext?.User?.Claims?.Select(claim => new KeyValuePair<string, string>(claim.Type, claim.Value)).ToList() ?? new();
    }

    public string UserId { get; }
    public string Role { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<KeyValuePair<string, string>> Claims { get; set; }

    public bool HasClaim(string claim, string value)
    {
        if (Claims.Count == 0) return false;
        return Claims.Any(claim => claim.Key.Equals(claim) && claim.Value == value);
    }

    public bool HasClaim(string value)
    {
        if (Claims.Count == 0) return false;
        return Claims.Any(claim => claim.Value == value);
    }
}
