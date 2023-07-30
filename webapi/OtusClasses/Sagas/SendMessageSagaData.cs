using OtusClasses.DataClasses.Dtos;

using Rebus.Sagas;

namespace OtusClasses.Sagas;

public class SendMessageSagaData : ISagaData
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    public string MessageId { get; set; }
    public DialogMessageDTO Data { get; set; }

    public bool IsMessageStoreOk { get;set; }

    public bool IsMessagePushOk { get; set; }

    public bool IsCountersUpdatedOk { get; set; }

    public bool IsCountersPushOk { get; set; }
}
