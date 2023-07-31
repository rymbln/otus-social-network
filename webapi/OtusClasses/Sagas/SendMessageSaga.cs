using Microsoft.Extensions.Logging;

using OtusClasses.Sagas.Events;

using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusClasses.Sagas;
public class SendMessageSaga : Saga<SendMessageSagaData>,
    IAmInitiatedBy<MessageCreatedEvent>,
    IHandleMessages<MessageSavedEvent>,
    IHandleMessages<MessagePushedEvent>,
    IHandleMessages<CountersUpdatedEvent>,
    IHandleMessages<CountersPushedEvent>,
    IHandleMessages<MessageFailEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<SendMessageSaga> _logger;

    public SendMessageSaga(IBus bus, ILogger<SendMessageSaga> logger)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _logger = logger;
    }

    public async Task Handle(MessageSavedEvent message)
    {
        Data.IsMessageStoreOk = message.IsSuccess;
        if (Data.IsMessageStoreOk)
            await _bus.Send(new PushMessageEvent(message.MessageId, message.Message, false));
        else
            await _bus.Send(new PushMessageFailedEvent(message.MessageId, message.Message));
    }

    public async Task Handle(MessagePushedEvent message)
    {
        Data.IsMessagePushOk = message.IsSuccess;
        await _bus.Send(new UpdateCountersEvent(message.MessageId, message.Message, false));
    }

    public async Task Handle(CountersUpdatedEvent message)
    {
        Data.IsCountersUpdatedOk = message.IsSuccess;
        if (Data.IsCountersUpdatedOk)
            await _bus.Send(new PushCountersEvent(message.MessageId, message.Data, message.Message.To, false));
        else
            await _bus.Send(new DeleteMessageEvent(message.MessageId, message.Message));
    }

    public Task Handle(CountersPushedEvent message)
    {
        Data.IsCountersPushOk = message.IsSuccess;

        MarkAsComplete();
        _logger.LogInformation("SendMessageSaga completed");
        return Task.CompletedTask;
    }

    public async Task Handle(MessageCreatedEvent message)
    {
        if (!IsNew) return;
        _logger.LogInformation("SendMessageSaga started");
        await _bus.Send(new SaveMessageEvent(message.MessageId, message.Message));
    }

    public Task Handle(MessageFailEvent message)
    {
        MarkAsComplete();
        _logger.LogInformation("SendMessageSaga completed");
        return Task.CompletedTask;
    }

    protected override void CorrelateMessages(ICorrelationConfig<SendMessageSagaData> config)
    {
        config.Correlate<MessageCreatedEvent>(m => m.MessageId, s => s.MessageId);
        config.Correlate<MessageSavedEvent>(m => m.MessageId, s => s.MessageId);
        config.Correlate<MessagePushedEvent>(m => m.MessageId, s => s.MessageId);
        config.Correlate<CountersUpdatedEvent>(m => m.MessageId, s => s.MessageId);
        config.Correlate<CountersPushedEvent>(m => m.MessageId, s => s.MessageId);
        config.Correlate<MessageFailEvent>(m => m.MessageId, s => s.MessageId);
    }

}
