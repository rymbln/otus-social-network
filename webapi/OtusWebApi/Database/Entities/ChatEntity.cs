namespace OtusSocialNetwork.Database.Entities;

public class ChatEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class ChatUser
{
    public Guid ChatId { get; set; }
    public Guid UserId { get; set; }
}

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public Guid UserId { get; set; }
    public string MessageText { get; set; }
    public bool IsNew { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ChatView
{
    public ChatView()
    {
    }

    public ChatView(string chatId, string chatName, string userId, string userName)
    {
        ChatId = chatId ?? throw new ArgumentNullException(nameof(chatId));
        ChatName = chatName ?? throw new ArgumentNullException(nameof(chatName));
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
    }

    public string ChatId { get; set; }
    public string ChatName { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
}

public class ChatMessageView
{
    public ChatMessageView()
    {
    }

    public ChatMessageView(string id, string chatId, string userId, string userName, string messageText, bool isNew, string timestamp)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        ChatId = chatId ?? throw new ArgumentNullException(nameof(chatId));
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        MessageText = messageText ?? throw new ArgumentNullException(nameof(messageText));
        IsNew = isNew;
        Timestamp = timestamp ?? throw new ArgumentNullException(nameof(timestamp));
    }

    public string Id { get; set; }
    public string ChatId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string MessageText { get; set; }
    public bool IsNew { get; set; }
    public string Timestamp { get; set; }
}