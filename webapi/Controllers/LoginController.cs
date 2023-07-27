using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Internals;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.DataClasses.Requests;
using OtusSocialNetwork.DataClasses.Responses;
using OtusSocialNetwork.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/login")]
public class LoginController : ControllerBase
{
    private readonly IDatabaseContext _db;
    private readonly IPasswordService _pass;
    private readonly IMapper _mapper;
    private readonly JWTSettings _jwtSettings;
    private readonly IPublishEndpoint _rabbit;

    public LoginController(IDatabaseContext db,  IPasswordService pass, IOptions<JWTSettings> jwtSettings,
        IMapper mapper, IPublishEndpoint rabbit)
    {
        _db = db;
        _pass = pass;
        _jwtSettings = jwtSettings.Value;
        _mapper = mapper;
        _rabbit = rabbit;
    }
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginReq data)
    {
        var login = await _db.GetLoginAsync(data.Id);
        if (login.isSuccess)
        {
            var profile = await _db.GetUserAsync(data.Id);
            var isPasswordOk = _pass.VerifyHashedPassword(login.account.Password, data.Password);
            if (isPasswordOk)
            {
                var jwt = GenerateJWToken(login.account.Id, profile.user.GetFullName());
                var token = new JwtSecurityTokenHandler().WriteToken(jwt);

                // Write to tarantool
                await _rabbit.Publish<INotificationFeedReload>(
                    new NotificationFeedReload(login.account.Id)
                    );

                return Ok(new LoginRes(token));
            }
        } else
        {
            return BadRequest(login.msg);
        }
        return BadRequest("No login");
    }

    private JwtSecurityToken GenerateJWToken(string userId, string username)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim> {
            new Claim("uid", userId),
            new Claim(ClaimTypes.NameIdentifier, userId ),
        new Claim(ClaimTypes.Name, username)};

        var jwtSecurityToken = new JwtSecurityToken(
            claims: claims,
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: signingCredentials) ;
        
        return jwtSecurityToken;
    }
}
