using OtusClasses.DataClasses;
using OtusClasses.DataClasses.Dtos;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusClasses.Sagas.Events;

public record MessageCreatedEvent(string MessageId, DialogMessageDTO Message);

public record SaveMessageEvent(string MessageId, DialogMessageDTO Message);
public record DeleteMessageEvent(string MessageId, DialogMessageDTO Message);
public record MessageSavedEvent(string MessageId, DialogMessageDTO Message, bool IsSuccess);

public record PushMessageFailedEvent(string MessageId, DialogMessageDTO Message);
public record MessageFailEvent(string MessageId, DialogMessageDTO Message);

public record PushMessageEvent(string MessageId, DialogMessageDTO Message, bool IsSuccess);
public record MessagePushedEvent(string MessageId, DialogMessageDTO Message, bool IsSuccess);

public record UpdateCountersEvent(string MessageId, DialogMessageDTO Message, bool IsSuccess);
public record CountersUpdatedEvent(string MessageId, List<ChatCounterDto> Data, DialogMessageDTO Message, bool IsSuccess);


public record PushCountersEvent(string MessageId, List<ChatCounterDto> Data, string ToId,  bool IsSuccess);
public record CountersPushedEvent(string MessageId, bool IsSuccess);

