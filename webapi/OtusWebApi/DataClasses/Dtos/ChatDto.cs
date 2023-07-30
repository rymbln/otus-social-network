namespace OtusSocialNetwork.DataClasses.Dtos
{
    public record ChatDTO(string Id, string Name, string UserId, string UserName)
    {

    }

    public record ChatForm(string UserId);
    public record ChatMessageForm(string Message);

    public record ChatItem(string Id, string Name, string[] UserIds);
    public record ChatSocket(string UserId, string ConnectionId);
    public record ChatMessageItem(string Id, string ChatId, string UserId, string Message, string Timestamp );
}
