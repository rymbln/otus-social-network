using System;
using AutoMapper;
using MassTransit;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.Tarantool;

namespace OtusSocialNetwork.Consumers;

public class NotificationFeedAddConsumer: IConsumer<INotificationFeedAdd>
{
    private readonly ITarantoolService _tarantool;

    public NotificationFeedAddConsumer(ITarantoolService tarantool)
    {
        _tarantool = tarantool;
    }

    public async Task Consume(ConsumeContext<INotificationFeedAdd> context)
    {
        foreach (var userId in context.Message.UserIds)
        {
            await _tarantool.WritePost(userId, context.Message.Post);
            Console.WriteLine($"INotificationFeedAdd event consumed. Message: {context.Message.PostId}");
        }
    }
}

