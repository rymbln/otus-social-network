using Confluent.Kafka;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

using OtusSocialNetwork.DataClasses.Notifications;
using OtusSocialNetwork.SignalHub;
using RabbitMQ.Client;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OtusSocialNetwork.Consumers;

public class PushFeedUpdateConsumer: IConsumer<IPushFeedUpdate>
{
    private readonly IHubContext<PostHub> _hub;

    public PushFeedUpdateConsumer(IHubContext<PostHub> hub)
    {
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
    }

    public async Task Consume(ConsumeContext<IPushFeedUpdate> context)
    {
        if (context.Message.ConnectionIds.Count > 0)
        {
            var data = new PostHubModel(context.Message.PostId, context.Message.Post.Text, context.Message.Post.AuthorUserId);
            await _hub.Clients.Clients(context.Message.ConnectionIds).SendAsync("Posted", data);
        }
        Console.WriteLine($"IPushFeedUpdate event consumed. Message: {context.Message.PostId}");
    }
}
