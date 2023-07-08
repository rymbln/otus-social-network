using System;
using AutoMapper;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.DataClasses.Requests;
using OtusSocialNetwork.DataClasses.Responses;
using OtusSocialNetwork.Services;
using OtusSocialNetwork.Tarantool;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
	private readonly IAuthenticatedUserService _auth;
	private readonly IDatabaseContext _db;
	private readonly IMapper _mapper;
	private readonly ITarantoolService _tarantool;
    private readonly IPublishEndpoint _rabbit;
    

    public PostController(IAuthenticatedUserService auth, IDatabaseContext db, IMapper mapper,
		ITarantoolService tarantool,
		IPublishEndpoint rabbit)
	{
		_auth = auth;
		_db = db;
		_mapper = mapper;
		_tarantool = tarantool;
		_rabbit = rabbit;

	}
 //   [HttpGet]
	//[Route("/feed/posted")]
 //   public IActionResult Get()
 //   {
 //       _hub.Clients.All.SendAsync("Posted", DataManager.GetData());
 //       return Ok(new { Message = "Request Completed" });
 //   }

    [HttpGet]
	public async Task<IActionResult> GetPosts()
	{
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

        var dbRes = await _db.GetPosts(_auth.UserId);
        if (!dbRes.isSuccess) return BadRequest(new ErrorRes(dbRes.msg));
        
        var res = _mapper.Map<List<PostDto>>(dbRes.posts.OrderByDescending(o => o.TimeStamp));

		// Write posts to tarantool
		//await _tarantool.WritePosts(_auth.UserId, res);

		return Ok(res);
    }

	[HttpGet("feed")]
	public async Task<IActionResult> GetFeed()
	{
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _tarantool.ReadPosts(_auth.UserId);

		return Ok(res);
    }

	[HttpGet("{id}")]
	public async Task<IActionResult> GetPost(string id)
	{
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var dbRes = await _db.GetPost(id, _auth.UserId);
        if (!dbRes.isSuccess) return BadRequest(new ErrorRes(dbRes.msg));

        var res = _mapper.Map<PostDto>(dbRes.post);
        return Ok(res);
    }

	[HttpPost("create")]
	public async Task<IActionResult> CreatePost(CreatePostReq req)
	{
		if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		// Add Post to collection
		var res = await _db.CreatePost(req.Text, _auth.UserId);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

		// Get Post 
		var postId = res.msg;
		var post = await _db.GetPost(postId);
        if (!post.isSuccess) return BadRequest(new ErrorRes(post.msg));
        var postDto = _mapper.Map<PostDto>(post.post);

		// Get Friends
		var friends = await _db.GetFriends(_auth.UserId);
        if (!friends.isSuccess) return BadRequest(new ErrorRes(friends.msg));

		// Write to tarantool
		await _rabbit.Publish<INotificationFeedAdd>(
			new NotificationFeedAdd(friends.data.Select(o => o.Id).ToList(), postId, postDto)
			);

		return Ok(new DefaultRes(res.msg));
	}

	[HttpPut("update")]
	public async Task<IActionResult> UpdatePost(UpdatePostReq req)
	{
		if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _db.UpdatePost(req.Id, _auth.UserId, req.Text);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));
        var post = await _db.GetPost(req.Id);
        if (!post.isSuccess) return BadRequest(new ErrorRes(post.msg));
        var postDto = _mapper.Map<PostDto>(post.post);

        // Write to tarantool
        await _rabbit.Publish<INotificationFeedUpdate>(
            new NotificationFeedUpdate(req.Id, postDto)
            );

        return Ok(new DefaultRes(res.msg));
    }

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeletePost(string id)
	{
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _db.DeletePost(id, _auth.UserId);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        // Write to tarantool
        await _rabbit.Publish<INotificationFeedDelete>(
            new NotificationFeedDelete(id)
            );

        return Ok(new DefaultRes(res.msg));
    }
}

