using System;
using Microsoft.AspNetCore.SignalR;
using OtusSignalR.Models;

namespace OtusSignalR.HubConfig;

public class ChartHub : Hub
{
    public async Task BroadcastChartData(List<ChartModel> data) =>
        await Clients.All.SendAsync("broadcastchartdata", data);
}

