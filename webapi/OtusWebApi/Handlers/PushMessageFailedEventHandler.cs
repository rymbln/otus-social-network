using Microsoft.AspNetCore.SignalR;

using OtusClasses.Sagas.Events;

using OtusSocialNetwork.SignalHub;
using OtusSocialNetwork.Tarantool;

using Rebus.Bus;
using Rebus.Handlers;

namespace OtusSocialNetwork.Handlers;

public class PushMessageFailedEventHandler: IHandleMessages<PushMessageFailedEvent>
{
    private readonly ILogger<PushMessageEventHandler> _logger;
    private readonly IBus _bus;
    private readonly IHubContext<ChatHub> _hub;
    private readonly ITarantoolService _tarantool;

    public PushMessageFailedEventHandler(ILogger<PushMessageEventHandler> logger, IBus bus, IHubContext<ChatHub> hub, ITarantoolService tarantool)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        _tarantool = tarantool ?? throw new ArgumentNullException(nameof(tarantool));
    }

    public async Task Handle(PushMessageFailedEvent message)
    {
        _logger.LogInformation("PushMessageFailedEventHandler: {@MessageId}", message.MessageId);

        // Get Connections for correspondent
        var data = await _tarantool.GetConnectedUsers(new List<string> { message.Message.From });
        if (data.Count > 0)
        {
            await _hub.Clients.Clients(data).SendAsync("Failed", new MessageHubModel(message.Message.From, "", message.Message.Text));

        }
        await _bus.Reply(new MessageFailEvent(message.MessageId, message.Message));
    }
}
