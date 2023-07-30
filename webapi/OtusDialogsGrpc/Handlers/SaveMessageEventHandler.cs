using OtusClasses.Sagas.Events;

using OtusDialogsGrpc.Database.Interfaces;

using Rebus.Bus;
using Rebus.Handlers;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OtusDialogsGrpc.Handlers;

public class SaveMessageEventHandler : IHandleMessages<SaveMessageEvent>
{
    private readonly ILogger<SaveMessageEventHandler> _logger;
    private readonly IBus _bus;
    private readonly ITarantoolService _tarantool;

    public SaveMessageEventHandler(ILogger<SaveMessageEventHandler> logger, IBus bus, ITarantoolService tarantool)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _tarantool = tarantool ?? throw new ArgumentNullException(nameof(tarantool));
    }

    public async Task Handle(SaveMessageEvent data)
    {
        try
        {
            _logger.LogInformation("SaveMessageEventHandler: {@MessageId}", data.MessageId);
            var res = await _tarantool.SendDialogMessage(data.Message.From, data.Message.To, data.Message.Text);
            await _bus.Reply(new MessageSavedEvent(data.MessageId, data.Message, true));
        } catch (Exception ex)
        {
            _logger.LogError(ex, "SaveMessageEventHandler Failed: {@MessageId}", data.MessageId);
            await _bus.Reply(new MessageSavedEvent(data.MessageId, data.Message, false));
        }
    }
}
