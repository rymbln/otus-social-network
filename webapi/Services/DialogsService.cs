using AutoMapper;

using Confluent.Kafka;

using Grpc.Net.Client;

using OtusClasses;
using OtusClasses.DataClasses.Dtos;

namespace OtusSocialNetwork.Services;

public class DialogsService : IDialogsService
{
    private readonly string connStr;
    private readonly IMapper _mapper;

    public DialogsService(IMapper mapper, IConfiguration config)
    {
        _mapper = mapper;
        connStr = config["DialogServiceHost"] ?? "";
    }

    public async Task<DialogMessageDTO> SendMessage(string from, string to, string message)
    {
        using var channel = GrpcChannel.ForAddress(connStr);
        var client = new Dialogs.DialogsClient(channel);
        var reply = await client.SendMessageAsync(new MessageRequest
        {
            From = from,
            To = to,
            Message = message
        });
        var res = new DialogMessageDTO(reply.Id, reply.From, reply.To, reply.Message, reply.Timestamp.ToDateTime());
        return res;

    }

    public async Task<List<DialogMessageDTO>> GetDialog(string from, string to)
    {
        using var channel = GrpcChannel.ForAddress(connStr);
        var client = new Dialogs.DialogsClient(channel);
        var reply = await client.GetMessagesAsync(new DialogRequest { UserId = from, CorrespondentId = to });

        var res = new List<DialogMessageDTO>();
        foreach (var item in reply.Messages.OrderByDescending(o => o.Timestamp))
        {
            res.Add(new DialogMessageDTO(string.Empty, item.From, item.To, item.Message, item.Timestamp.ToDateTime()));
        }

        return res;
    }
}
