using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.Services;
using OtusSocialNetwork.Tarantool;

namespace OtusSocialNetwork.SignalHub;

[Authorize]
public class PostHub: Hub
{
    private readonly IAuthenticatedUserService authenticatedUserService;
    private readonly ITarantoolService _tarantool;
    private readonly IDatabaseContext _db;

    public PostHub(IAuthenticatedUserService auth, ITarantoolService tarantool,
        IDatabaseContext db)
	{
        authenticatedUserService = auth;
        _tarantool = tarantool;
        _db = db;
	}

    [Authorize]
    public async Task Posted(PostHubModel data)
    {
        var friends = await _db.GetFriends(data.AuthorUserId);
        var friendIds = friends.data.Select(o => o.Id).ToList();
        var connections = await _tarantool.GetConnectedUsers(friendIds);
        if (connections.Count > 0)
        await Clients.Clients(connections).SendAsync("Posted", data);
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = authenticatedUserService.UserId;

        await _tarantool.AddUserSocket(userId, connectionId);

        await Clients.Caller.SendAsync("Connected", "You are now connected to the WebSocket.");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _tarantool.DeleteUserSocket(string.Empty, Context.ConnectionId);
    }

    public string GetConnectionId() => Context.ConnectionId;
}

public class PostHubModel
{
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
