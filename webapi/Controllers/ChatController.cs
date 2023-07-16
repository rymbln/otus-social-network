using AutoMapper;

using MassTransit;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Responses;
using OtusSocialNetwork.Services;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ChatController: ControllerBase
{
    private readonly IAuthenticatedUserService _auth;
    private readonly IDatabaseContext _db;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _rabbit;

    public ChatController(IAuthenticatedUserService auth,
        IDatabaseContext db,
        IMapper mapper,
        IPublishEndpoint rabbit) 
    {
        _auth = auth;
        _db = db;
        _mapper = mapper;
        _rabbit = rabbit;
    }

    [HttpGet]
    public async Task<IActionResult> GetChats()
    {
        var res = await _db.GetChats(_auth.UserId);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        return Ok(res.chats);
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat(ChatForm form)
    {
        var res = await _db.CreateChat(_auth.UserId, form.UserId);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        return Ok(res);
    }

    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteChat(string chatId)
    {
        var res = await _db.DeleteChat(chatId, _auth.UserId);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        return Ok(res);
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(string chatId)
    {
        var res = await _db.GetMessages(_auth.UserId, chatId);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        return Ok(res.messages);
    }

    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessages(string chatId, ChatMessageForm form)
    {
        var res = await _db.CreateMessage(chatId, _auth.UserId, form.Message);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        return Ok(res);
    }
}
