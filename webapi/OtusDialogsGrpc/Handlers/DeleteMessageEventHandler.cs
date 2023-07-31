using OtusClasses.Sagas.Events;

using OtusDialogsGrpc.Database.Interfaces;

using Rebus.Bus;
using Rebus.Handlers;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OtusDialogsGrpc.Handlers;

public class DeleteMessageEventHandler : IHandleMessages<DeleteMessageEvent>
{
    private readonly ILogger<DeleteMessageEventHandler> _logger;
    private readonly IBus _bus;
    private readonly ITarantoolService _tarantool;

    public DeleteMessageEventHandler(ILogger<DeleteMessageEventHandler> logger, IBus bus, ITarantoolService tarantool)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _tarantool = tarantool ?? throw new ArgumentNullException(nameof(tarantool));
    }

    public async Task Handle(DeleteMessageEvent message)
    {
        _logger.LogInformation("DeleteMessageEventHandler: {@MessageId}", message.MessageId);
        await _tarantool.DeleteDialogMessage(message.MessageId);
        await _bus.Reply(new PushMessageFailedEvent(message.MessageId, message.Message));
    }
}
