using System;
using System.Text.Json;
using AutoMapper;
using MassTransit;
using OtusSocialNetwork.Database;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.Tarantool;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OtusSocialNetwork.Consumers;

public class NotificationFeedReloadConsumer : IConsumer<INotificationFeedReload>
{
    private readonly ITarantoolService _tarantool;
    private readonly IDatabaseContext _db;
    private readonly IMapper _mapper;

    public NotificationFeedReloadConsumer(ITarantoolService tarantool, IDatabaseContext db, IMapper mapper)
    {
        _tarantool = tarantool;
        _db = db;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<INotificationFeedReload> context)
    {
        var userId = context.Message.UserId;
        // Get posts
        var dbres = await _db.GetFeed(userId, 1000);
        var posts = _mapper.Map<List<PostDto>>(dbres.posts);
        // SAve feed
        await _tarantool.WritePosts(userId, posts);

        Console.WriteLine($"INotificationFeedReload event consumed. Message: {userId}");
    }
}

