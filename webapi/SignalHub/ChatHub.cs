using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.Services;
using OtusSocialNetwork.Tarantool;

namespace OtusSocialNetwork.SignalHub;

public class ChatHub: Hub
{
    private readonly IAuthenticatedUserService _auth;
    private readonly IDatabaseContext _db;
    private readonly ITarantoolService _tarantool;
    private readonly IConfiguration _config;

    public ChatHub(
        IAuthenticatedUserService auth,
           IDatabaseContext db,
           ITarantoolService tarantool,
           IConfiguration config)
    {
        _auth = auth;
        _db = db;
        _config = config;
        _tarantool = tarantool;
    }

    [Authorize]
    public async Task Sended(MessageHubModel data)
    {
        var connections = await _tarantool.GetUserChatSockets(data.UserId);
        if (connections.Count > 0)
        {
            var connIds = connections.Select(x => x.ConnectionId).ToList();
            await Clients.Clients(connIds).SendAsync("Received", data);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = _auth.UserId;

        await _tarantool.AddUserChatSocket(userId, connectionId);

        await Clients.Caller.SendAsync("Connected", "You are now connected to the WebSocket.");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _tarantool.DeleteUserChatSocket(Context.ConnectionId);
    }

    public string GetConnectionId() => Context.ConnectionId;
}

public class MessageHubModel
{
    public MessageHubModel()
    {
    }

    public MessageHubModel(string userId, string chatName, string message)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        ChatName = chatName ?? throw new ArgumentNullException(nameof(chatName));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public string UserId { get; set; }
    public string ChatName { get; set; }
    public string Message { get; set; }
}