using System;
using MassTransit;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.Tarantool;

namespace OtusSocialNetwork.Consumers;

public class NotificationFeedDeleteConsumer : IConsumer<INotificationFeedDelete>
{
    private readonly ITarantoolService _tarantool;

    public NotificationFeedDeleteConsumer(ITarantoolService tarantool)
	{
        _tarantool = tarantool;
    }

    public async Task Consume(ConsumeContext<INotificationFeedDelete> context)
    {
        await _tarantool.DeletePost(context.Message.PostId);
        Console.WriteLine($"INotificationFeedDelete event consumed. Message: {context.Message.PostId}");
    }
}

