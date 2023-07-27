using System;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.DataClasses.Responses;
using OtusSocialNetwork.Services;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class FriendController: ControllerBase
{
    private readonly IAuthenticatedUserService _auth;
    private readonly IDatabaseContext _db;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _rabbit;


    public FriendController(IAuthenticatedUserService auth, IDatabaseContext db, IMapper mapper, IPublishEndpoint rabbit)
	{
        _auth = auth;
        _db = db;
        _mapper = mapper;
        _rabbit = rabbit;
	}

    [HttpPut("set/{user_id}")]
    public async Task<IActionResult> SetFriend(string user_id)
    {
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

        var res = await _db.AddFriend(_auth.UserId, user_id);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        await _rabbit.Publish<INotificationFeedReload>(
               new NotificationFeedReload(_auth.UserId)
               );
        return Ok(res);
    }

    [HttpPut("delete/{user_id}")]
    public async Task<IActionResult> DeleteFriend(string user_id)
    {
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

        var res = await _db.DeleteFriend(_auth.UserId, user_id);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        
        await _rabbit.Publish<INotificationFeedReload>(
            new NotificationFeedReload(_auth.UserId)
            );

        return Ok(res);
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

        var dbres = await _db.GetFriends(_auth.UserId);
        if (!dbres.isSuccess) return BadRequest(new ErrorRes(dbres.msg));

        var res = _mapper.Map<List<FriendDto>>(dbres.data);
        return Ok(res);
    }

    [HttpGet("search/{query}")]
    public async Task<IActionResult> SearchFriends(string query)
    { 
        var dbres = await _db.SearchFriends(query);
        if (!dbres.isSuccess) return BadRequest(new ErrorRes(dbres.msg));

        var res = _mapper.Map<List<FriendDto>>(dbres.data);

        return Ok(res);
    }
}

