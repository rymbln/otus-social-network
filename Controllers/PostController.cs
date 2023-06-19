using System;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Requests;
using OtusSocialNetwork.DataClasses.Responses;
using OtusSocialNetwork.Services;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("post")]
public class PostController : ControllerBase
{
	private readonly IAuthenticatedUserService _auth;
	private readonly IDatabaseContext _db;
	private readonly IMapper _mapper;
	public PostController(IAuthenticatedUserService auth, IDatabaseContext db, IMapper mapper)
	{
		_auth = auth;
		_db = db;
		_mapper = mapper;
	}

	[HttpGet]
	public async Task<IActionResult> GetPosts()
	{
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

        var dbRes = await _db.GetPosts(_auth.UserId);
        if (!dbRes.isSuccess) return BadRequest(new ErrorRes(dbRes.msg));
        
        var res = _mapper.Map<List<PostDto>>(dbRes.posts);
		return Ok(res);
    }

	[HttpGet("feed")]
	public async Task<IActionResult> GetFeed()
	{
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _db.GetFeed(_auth.UserId, 1000);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

		return Ok(res.posts);
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

		var res = await _db.CreatePost(req.Text, _auth.UserId);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

		return Ok(new DefaultRes(res.msg));
	}

	[HttpPut("update")]
	public async Task<IActionResult> UpdatePost(UpdatePostReq req)
	{
		if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _db.UpdatePost(req.Id, _auth.UserId, req.Text);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        return Ok(new DefaultRes(res.msg));
    }

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeletePost(string id)
	{
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _db.DeletePost(id, _auth.UserId);
        if (!res.isSuccess) return BadRequest(new ErrorRes(res.msg));

        return Ok(new DefaultRes(res.msg));
    }
}

