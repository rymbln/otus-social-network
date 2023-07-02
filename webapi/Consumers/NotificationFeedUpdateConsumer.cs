using System;
using AutoMapper;
using MassTransit;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.Tarantool;

namespace OtusSocialNetwork.Consumers;

public class NotificationFeedUpdateConsumer : IConsumer<INotificationFeedUpdate>
{
    private readonly ITarantoolService _tarantool;

    public NotificationFeedUpdateConsumer(ITarantoolService tarantool)
	{
        _tarantool = tarantool;
	}

    public async Task Consume(ConsumeContext<INotificationFeedUpdate> context)
    {
        await _tarantool.UpdatePost(context.Message.Post);
        Console.WriteLine($"INotificationFeedUpdate event consumed. Message: {context.Message.PostId}");
    }
}

