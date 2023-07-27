using AutoMapper;

using Confluent.Kafka;

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

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAuthenticatedUserService _auth;
    private readonly IDatabaseContext _db;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _rabbit;
    private readonly IHubContext<ChatHub> _hub;
    private readonly IConfiguration _config;
    private readonly bool _isPostgres;

    public ChatController(IAuthenticatedUserService auth,
        IDatabaseContext db,
        IMapper mapper,
        IPublishEndpoint rabbit,
        IConfiguration config,
        IHubContext<ChatHub> hub)
    {
        _auth = auth;
        _db = db;
        _mapper = mapper;
        _rabbit = rabbit;
        _hub = hub;
        _config = config;
        _isPostgres = _config["ChatMode"] == "Postgres";
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
        }
        return Ok(res.OrderBy(o => o.Timestamp));
    }

    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessages(string chatId, ChatMessageForm form)
    {
        var chatName = string.Empty;
        var messageText = form.Message;
        var correspondent = string.Empty;
        
        if (_isPostgres)
        {
            var chat = await _db.GetChat(chatId, _auth.UserId);
            if (chat.chat != null)
            {
                chatName = chat.chat.ChatName;
                correspondent = chat.chat.UserId;
            }
            var res = await _db.CreateMessage(chatId, _auth.UserId, form.Message);
            if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));
        }
        else
        {
          
        }

        // Send notifications
      
      
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
        }
        return Ok();
    }
}
