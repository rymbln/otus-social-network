using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Requests;
using OtusSocialNetwork.Services;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class PostController : ControllerBase
{
	private readonly IAuthenticatedUserService _auth;
	private readonly IDatabaseContext _db;
	public PostController(IAuthenticatedUserService auth, IDatabaseContext db)
	{
		_auth = auth;
		_db = db;
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetPost(string id)
	{
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _db.GetPost(id, _auth.UserId);
        return res.isSuccess ? Ok(res.post) : BadRequest(res.msg);
    }

	[HttpPost("create")]
	public async Task<IActionResult> CreatePost(CreatePostReq req)
	{
		if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _db.CreatePost(req.Text, _auth.UserId);

		return res.isSuccess ? Ok(res.msg) : BadRequest(res.msg);
	}

	[HttpPut("update")]
	public async Task<IActionResult> UpdatePost(UpdatePostReq req)
	{
		if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _db.UpdatePost(req.Id, _auth.UserId, req.Text);

		return res.isSuccess ? Ok(res.msg) : BadRequest(res.msg);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeletePost(string id)
	{
        if (string.IsNullOrEmpty(_auth.UserId)) return BadRequest("User not found");

		var res = await _db.DeletePost(id, _auth.UserId);

        return res.isSuccess ? Ok(res.msg) : BadRequest(res.msg);
    }
}

