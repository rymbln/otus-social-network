using MassTransit;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.Tarantool;

namespace OtusSocialNetwork.Consumers;

public class NotificationChatSendedConsumerPostgres: IConsumer<INotificationChatSended>
{
    private readonly IDatabaseContext _db;

    public NotificationChatSendedConsumerPostgres(IDatabaseContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Consume(ConsumeContext<INotificationChatSended> context)
    {
        Console.WriteLine($"INotificationChatSended event consumed. ChatId: {context.Message.ChatId}.  UserId: {context.Message.ChatId}");
    }
}
