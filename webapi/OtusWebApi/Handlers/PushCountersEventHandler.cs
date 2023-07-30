using Microsoft.AspNetCore.SignalR;
using OtusClasses.Sagas.Events;

using OtusSocialNetwork.SignalHub;
using OtusSocialNetwork.Tarantool;

using Rebus.Bus;
using Rebus.Handlers;

namespace OtusSocialNetwork.Handlers;

public class PushCountersEventHandler : IHandleMessages<PushCountersEvent>
{
    private readonly ILogger<PushCountersEventHandler> _logger;
    private readonly IBus _bus;
    private readonly IHubContext<ChatHub> _hub;
    private readonly ITarantoolService _tarantool;

    public PushCountersEventHandler(
        ILogger<PushCountersEventHandler> logger, 
        IBus bus, IHubContext<ChatHub> hub,
        ITarantoolService tarantool)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        _tarantool = tarantool;
    }

    public async Task Handle(PushCountersEvent message)
    {
        _logger.LogInformation("PushCountersEventHandler: {@MessageId}", message.MessageId);

        // Get Connections for correspondent
        var data = await _tarantool.GetConnectedUsers(new List<string> { message.ToId });
        if (data.Count > 0)
        {
            await _hub.Clients.Clients(data).SendAsync("Unreads", new CountersHubModel(message.Data));

        }
        await _bus.Reply(new CountersPushedEvent(message.MessageId, true));
    }
}
