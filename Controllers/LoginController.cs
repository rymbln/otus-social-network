using Microsoft.AspNetCore.Mvc;
using OtusSocialNetwork.DataClasses.Requests;

namespace OtusSocialNetwork.Controllers;

public class LoginController
{
    [HttpPost()]
    public async Task<IActionResult> Login(LoginReq data)
    {
        throw new NotImplementedException();
    }
}
