using Confluent.Kafka;

using Microsoft.AspNetCore.SignalR;

using OtusClasses.Sagas.Events;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.SignalHub;
using OtusSocialNetwork.Tarantool;

using Rebus.Bus;
using Rebus.Handlers;

namespace OtusSocialNetwork.Handlers;

public class PushMessageEventHandler : IHandleMessages<PushMessageEvent>
{
    private readonly ILogger<PushMessageEventHandler> _logger;
    private readonly IBus _bus;
    private readonly IHubContext<ChatHub> _hub;
    private readonly ITarantoolService _tarantool;

    public PushMessageEventHandler(ILogger<PushMessageEventHandler> logger, IBus bus, IHubContext<ChatHub> hub, ITarantoolService tarantool)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _tarantool = tarantool;
        _hub = hub;
    }

    public async  Task Handle(PushMessageEvent message)
    {
        _logger.LogInformation("PushMessageEventHandler: {@MessageId}", message.MessageId);

        // Get Connections for correspondent
        var data = await _tarantool.GetConnectedUsers(new List<string> { message.Message.To });
        if (data.Count > 0)
        {
            await _hub.Clients.Clients(data).SendAsync("Received", new MessageHubModel(message.Message.From, "", message.Message.Text));

        }
        await _bus.Reply(new MessagePushedEvent(message.MessageId, message.Message, true));
    }
}
