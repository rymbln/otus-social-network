using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OtusClasses.DataClasses.Dtos;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Responses;
using OtusSocialNetwork.Services;
using OtusSocialNetwork.Tarantool;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class DialogController : ControllerBase
{
    private readonly IAuthenticatedUserService _auth;
    private readonly IDatabaseContext _db;
    private readonly ITarantoolService _tarantool;
    private readonly IConfiguration _config;
    private readonly IDialogsService _dialogs;
    private readonly bool _isPostgres;

    public DialogController(IAuthenticatedUserService auth,
        IDatabaseContext db,
        ITarantoolService tarantool,
        IConfiguration config,
        IDialogsService dialogs
      )
    {
        _auth = auth;
        _db = db;
        _tarantool = tarantool;
        _config = config;
        _isPostgres = _config["ChatMode"] == "Postgres";
        _dialogs = dialogs;
    }

    [Authorize]
    [HttpPost("{userId}/send")]
    public async Task<IActionResult> Send(Guid userId, DialogMessageForm form)
    {
        var res = await _dialogs.SendMessage(_auth.UserId, userId.ToString(), form.Text);
        return Ok(res);
        // ---
        //if (_isPostgres)
        //{
        //    var res = await _db.SendDialogMessage(_auth.UserId, userId.ToString(), form.Text );
        //    if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));
        //}
        //else
        //{
        //    await _tarantool.SendDialogMessage(_auth.UserId, userId.ToString(), form.Text);
        //}
        //return Ok(new DialogMessageDTO(_auth.UserId, userId.ToString(), form.Text, DateTime.UtcNow));
    }

    [Authorize]
    [HttpPost("{userId}/list")]
    public async Task<IActionResult> Get(Guid userId)
    {
        var res = await _dialogs.GetDialog(_auth.UserId, userId.ToString());
        return Ok(res);
        // --
        //var res = new List<DialogMessageDTO>();
        //if (_isPostgres)
        //{
        //    var data = await _db.GetDialogMessages(_auth.UserId, userId.ToString());
        //    if (!data.isSuccess) return BadRequest(new ErrorRes(data.msg));

        //    res.AddRange(data.messages);
        //}
        //else
        //{
        //    var data = await _tarantool.GetDialogMessages(_auth.UserId, userId.ToString());
        //    res.AddRange(data);

        //}
        //return Ok(res);
    }
}
