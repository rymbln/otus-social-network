using AutoMapper;

using MassTransit;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.Database.Entities;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Responses;
using OtusSocialNetwork.Services;
using OtusSocialNetwork.SignalHub;
using OtusSocialNetwork.Tarantool;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAuthenticatedUserService _auth;
    private readonly IDatabaseContext _db;
    private readonly ITarantoolService _tarantool;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _rabbit;
    private readonly IHubContext<ChatHubPostgres> _hub;
    private readonly IConfiguration _config;
    private readonly bool _isPostgres;

    public ChatController(IAuthenticatedUserService auth,
        IDatabaseContext db,
        ITarantoolService tarantool,
        IMapper mapper,
        IPublishEndpoint rabbit,
        IConfiguration config,
        IHubContext<ChatHubPostgres> hub)
    {
        _auth = auth;
        _db = db;
        _tarantool = tarantool;
        _mapper = mapper;
        _rabbit = rabbit;
        _hub = hub;
        _config = config;
        _isPostgres = _config["ChatMode"] == "Postgres";
    }

    [HttpGet]
    [Route("/feed/sended")]
    public IActionResult Get()
    {
        _hub.Clients.All.SendAsync("Posted", DataManager.GetData());
        return Ok(new { Message = "Request Completed" });
    }


    [HttpGet]
    public async Task<IActionResult> GetChats()
    {
        var res = new List<ChatView>();

        if (_isPostgres) {
            var data = await _db.GetChats(_auth.UserId);
            if (!data.isSuccess) return BadRequest(new ErrorRes(data.msg));

            res = data.chats;
        } else
        {
            var data = await _tarantool.GetChats(_auth.UserId);
            foreach (var item in data)
            {
                var friend = item.UserIds.Where( o=> o != _auth.UserId).First();
                res.Add(new ChatView(item.Id, item.Name, friend, string.Empty));
            }
        }

        return Ok(res);
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat(ChatForm form)
    {
        if (_isPostgres)
        {
            var res = await _db.CreateChat(_auth.UserId, form.UserId);
            if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));
        } else
        {
            await _tarantool.CreateChat(Guid.NewGuid().ToString(), $"Chat {DateTime.Now}", new List<string> { _auth.UserId, form.UserId });
        }
        return Ok();
    }

    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteChat(string chatId)
    {
        if (_isPostgres)
        {
            var res = await _db.DeleteChat(chatId, _auth.UserId);
            if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));
        } else
        {
            await _tarantool.DeleteChat(chatId);
        }
        return Ok();
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(string chatId)
    {
        var res = new List<ChatMessageView>();
        if (_isPostgres)
        {
            var data = await _db.GetMessages(_auth.UserId, chatId);
            if (!data.isSuccess) return BadRequest(new ErrorRes(data.msg));
        }
        else
        {
            var data = await _tarantool.GetMessages(chatId);
            foreach (var item in data)
            {
                res.Add(new ChatMessageView(item.Id, item.ChatId, item.UserId, string.Empty, item.Message, false, item.Timestamp));
            }
        }
        return Ok(res);
    }

    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessages(string chatId, ChatMessageForm form)
    {
        if (_isPostgres)
        {
            var res = await _db.CreateMessage(chatId, _auth.UserId, form.Message);
            if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));
        }
        else
        {
            await _tarantool.AddMessage(Guid.NewGuid().ToString(), chatId, _auth.UserId, form.Message);
        }
        return Ok();
    }

    [HttpDelete("{chatId}/messages/{id}")]
    public async Task<IActionResult> DeleteMessage(string chatId, string id)
    {
        if (_isPostgres)
        {
            var res = await _db.DeleteMessage(chatId, _auth.UserId, id);
            if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));
        } else
        {
            await _tarantool.DeleteMessage(id);
        }
        return Ok();
    }
}
