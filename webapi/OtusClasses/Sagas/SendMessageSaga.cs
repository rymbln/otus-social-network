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
    IHandleMessages<MessagePushedEvent>
    //IHandleMessages<CountsUpdatedEvent>,
    //IHandleMessages<CountsPushedEvent>
{
    private readonly IBus _bus;

    public SendMessageSaga(IBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }

    public async Task Handle(MessageSavedEvent message)
    {
        Data.IsMessageStoreOk = message.IsSuccess;

        await _bus.Send(new PushMessageEvent(message.MessageId, message.Message, false));
    }

    public async Task Handle(MessagePushedEvent message)
    {
        Data.IsMessagePushOk = message.IsSuccess;
        await _bus.Send(new UpdateCountsEvent(message.MessageId, message.Message, false));
    }

    public async Task Handle(CountsUpdatedEvent message)
    {
        Data.IsCountersUpdatedOk = message.IsSuccess;
        await _bus.Send(new PushCountersEvent(message.MessageId, message.Data, message.Message.To, false));
    }

    public Task Handle(CountersPushedEvent message)
    {
        Data.IsCountersPushOk = message.IsSuccess;

        MarkAsComplete();
        return Task.CompletedTask;
    }

    public async Task Handle(MessageCreatedEvent message)
    {
        if (!IsNew) return;

        await _bus.Send(new SaveMessageEvent(message.MessageId, message.Message));
    }

    protected override void CorrelateMessages(ICorrelationConfig<SendMessageSagaData> config)
    {
        config.Correlate<MessageCreatedEvent>(m => m.MessageId, s => s.MessageId);
        config.Correlate<MessageSavedEvent>(m => m.MessageId, s => s.MessageId);
        config.Correlate<MessagePushedEvent>(m => m.MessageId, s => s.MessageId);
        //config.Correlate<CountsUpdatedEvent>(m => m.MessageId, s => s.MessageId);
        //config.Correlate<CountsPushedEvent>(m => m.MessageId, s => s.MessageId);
    }

}
