using System;
using AutoMapper;
using MassTransit;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.Tarantool;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OtusSocialNetwork.Consumers;

public class NotificationFeedAddConsumer: IConsumer<INotificationFeedAdd>
{
    private readonly ITarantoolService _tarantool;
    private readonly IDatabaseContext _db;

    public NotificationFeedAddConsumer(ITarantoolService tarantool, IDatabaseContext db)
    {
        _tarantool = tarantool;
        _db = db;
    }

    public async Task Consume(ConsumeContext<INotificationFeedAdd> context)
    {
        Console.WriteLine($"INotificationFeedAdd event consumed. Message: {context.Message.PostId}");
        foreach (var userId in context.Message.UserIds)
        {
            await _tarantool.WritePost(userId, context.Message.Post);
        }

        // Send notifications for connected users
        var friends = await _db.GetFriends(context.Message.Post.AuthorUserId);
        var friendIds = friends.data.Select(o => o.Id).ToList();
        var connections = await _tarantool.GetConnectedUsers(friendIds);
        if (connections.Count > 0)
        {
            var data = new PushFeedUpdate(context.Message.UserIds, connections, context.Message.PostId, context.Message.Post);
            await context.Publish(data);
        }
    }
}

