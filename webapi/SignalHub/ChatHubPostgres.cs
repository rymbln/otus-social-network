using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.Services;

namespace OtusSocialNetwork.SignalHub;

public class ChatHubPostgres: Hub
{
    private readonly IAuthenticatedUserService _auth;
    private readonly IDatabaseContext _db;
    private readonly IConfiguration _config;

    public ChatHubPostgres(
        IAuthenticatedUserService auth,
           IDatabaseContext db,
           IConfiguration config)
    {
        _auth = auth;
        _db = db;
        _config = config;
    }

    [Authorize]
    public async Task Sended(MessageHubModel data)
    {
        //var chat = 
        //var friends = await _db.GetFriends(data.AuthorUserId);
        //var friendIds = friends.data.Select(o => o.Id).ToList();
        //var connections = await _tarantool.GetConnectedUsers(friendIds);
        //if (connections.Count > 0)
        //    await Clients.Clients(connections).SendAsync("Posted", data);
    }
    public async Task Received(MessageHubModel data)
    {

    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = _auth.UserId;

     //   await _tarantool.AddUserSocket(userId, connectionId);

        await Clients.Caller.SendAsync("Connected", "You are now connected to the WebSocket.");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        //await _tarantool.DeleteUserSocket(string.Empty, Context.ConnectionId);
    }

    public string GetConnectionId() => Context.ConnectionId;
}

public class MessageHubModel
{
    public string ChatId { get; set; }
    public string Message { get; set; }
}