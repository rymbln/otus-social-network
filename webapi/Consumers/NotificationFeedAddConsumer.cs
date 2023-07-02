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
    private readonly IDatabaseContext _db;
    private readonly IMapper _mapper;

    public NotificationFeedAddConsumer(ITarantoolService tarantool, IDatabaseContext db, IMapper mapper)
    {
        _tarantool = tarantool;
        _db = db;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<INotificationFeedAdd> context)
    {
        foreach (var userId in context.Message.UserIds)
        {
            await _tarantool.WritePost(userId, context.Message.Post);
        }
    }
}

