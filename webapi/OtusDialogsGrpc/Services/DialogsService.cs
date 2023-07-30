using AutoMapper;

using Google.Protobuf.Collections;

using Grpc.Core;

using OtusClasses;
using OtusClasses.DataClasses.Dtos;

using OtusDialogsGrpc.Database.Interfaces;

using static OtusClasses.DialogReply.Types;

namespace OtusDialogsGrpc.Services;

public class DialogsService: Dialogs.DialogsBase
{
    private readonly ILogger<DialogsService> _logger;
    private readonly ITarantoolService _tarantool;
    private readonly IMapper _mapper;
    public DialogsService(
        ILogger<DialogsService> logger,
        ITarantoolService tarantool,
        IMapper mapper)
    {
        _logger = logger;
        _tarantool = tarantool;
        _mapper = mapper;
    }

    public override async Task<MessageReply> SendMessage(MessageRequest request, ServerCallContext context)
    {
        _logger.LogInformation(request.ToString());
        var data = await _tarantool.SendDialogMessage(request.From, request.To, request.Message);

        var reply = _mapper.Map<MessageReply>(data);
        return reply;
    

    }
/// <inheritdoc/>

    public override async Task<DialogReply> GetMessages(DialogRequest request, ServerCallContext context)
    {
        _logger.LogInformation(request.ToString());
        var data = await _tarantool.GetDialogMessages(request.UserId, request.CorrespondentId);
        var reply = new DialogReply();
        foreach (var item in data)
        {
            reply.Messages.Add(_mapper.Map<DialogMessage>(item));
        }
        return reply;
    }


}
