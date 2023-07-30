using OtusChatCounters.Database;

using OtusClasses.Sagas.Events;

using Rebus.Bus;
using Rebus.Handlers;

namespace OtusChatCounters.Handlers;

public class UpdateCountsEventHandler: IHandleMessages<UpdateCountsEvent>
{
    private readonly ILogger<UpdateCountsEventHandler> _logger;
    private readonly IBus _bus;
    private readonly ITarantoolService _tarantool;

    public UpdateCountsEventHandler(ILogger<UpdateCountsEventHandler> logger, IBus bus, ITarantoolService tarantool)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _tarantool = tarantool ?? throw new ArgumentNullException(nameof(tarantool));
    }

    public async Task Handle(UpdateCountsEvent message)
    {
        _logger.LogInformation("UpdateCountsEventHandler: {@MessageId}", message.MessageId);
        var data = await _tarantool.SetUnreadsCount(message.Message.From, message.Message.To);
        await _bus.Reply(new CountsUpdatedEvent(message.MessageId, data, message.Message, true));
    }

}
