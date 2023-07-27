using System;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.Services;

namespace OtusSocialNetwork.SignalHub;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PostHub: Hub
{
    private readonly IAuthenticatedUserService authenticatedUserService;
    private readonly IDatabaseContext _db;

    public PostHub(IAuthenticatedUserService auth, 
        IDatabaseContext db)
	{
        authenticatedUserService = auth;
        _db = db;
	}

    [Authorize]
    public async Task Posted(PostHubModel data)
    {
        var friends = await _db.GetFriends(data.AuthorUserId);
        var friendIds = friends.data.Select(o => o.Id).ToList();
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = authenticatedUserService.UserId;


        await Clients.Caller.SendAsync("Connected", "You are now connected to the WebSocket.");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
    }

    public string GetConnectionId() => Context.ConnectionId;
}

public class PostHubModel
{
    public PostHubModel()
    {
    }

    public PostHubModel(string postId, string postText, string authorUserId)
    {
        PostId = postId ?? throw new ArgumentNullException(nameof(postId));
        PostText = postText ?? throw new ArgumentNullException(nameof(postText));
        AuthorUserId = authorUserId ?? throw new ArgumentNullException(nameof(authorUserId));
    }

    public string PostId { get; set; }
    public string PostText { get; set; }
    public string AuthorUserId { get; set; }

}

// TODO: delete
public class ChartModel
{
    public List<int> Data { get; set; }
    public string? Label { get; set; }
    public string? BackgroundColor { get; set; }
    public ChartModel()
    {
        Data = new List<int>();
    }
}
// TODO:Delete
public class ChartHub : Hub
{
    public async Task BroadcastChartData(List<ChartModel> data) =>
        await Clients.All.SendAsync("broadcastchartdata", data);

    public async Task BroadcastChartData(List<ChartModel> data, string connectionId) =>
    await Clients.Client(connectionId).SendAsync("broadcastchartdata", data);


    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", "You are now connected to the WebSocket.");
        await base.OnConnectedAsync();
    }

    public string GetConnectionId() => Context.ConnectionId;
}
