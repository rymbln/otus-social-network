using System;
using Microsoft.AspNetCore.SignalR;

namespace OtusSocialNetwork.SignalHub;

public class PostHub: Hub
{
	public PostHub()
	{
	}
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
