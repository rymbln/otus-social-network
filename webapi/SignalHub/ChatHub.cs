using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.Services;

namespace OtusSocialNetwork.SignalHub;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatHub: Hub
{
    private readonly IAuthenticatedUserService _auth;
    private readonly IDatabaseContext _db;
    private readonly IConfiguration _config;

    public ChatHub(
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
       
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = _auth.UserId;


        await Clients.Caller.SendAsync("Connected", "You are now connected to the WebSocket.");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
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