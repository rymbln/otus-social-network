namespace OtusSocialNetwork.DataClasses.Dtos
{
    public record ChatDTO(string Id, string Name, string UserId, string UserName)
    {

    }

    public record ChatForm(string UserId);
    public record ChatMessageForm(string Message);
}
