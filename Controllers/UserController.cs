using Microsoft.AspNetCore.Mvc;

using OtusSocialNetwork.DataClasses.Requests;

namespace OtusSocialNetwork.Controllers;

public class UserController
{
    [HttpPost("user/register")]
    public async Task<IActionResult> Register(RegisterReq data)
    {
        throw new NotImplementedException();
    }

    [HttpGet("user/get/{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        throw new NotImplementedException();
    }

}
