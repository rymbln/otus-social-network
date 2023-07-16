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
    public string ChatId { get; set; }
    public string ChatName { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
}

public class ChatMessageView
{
    public string Id { get; set; }
    public string ChatId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string MessageText { get; set; }
    public bool IsNew { get; set; }
    public string Timestamp { get; set; }
}