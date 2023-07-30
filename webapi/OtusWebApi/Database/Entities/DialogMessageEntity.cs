namespace OtusSocialNetwork.Database.Entities;

public class DialogMessageEntity
{
    public string Id { get; set; }
    public string FromUserId { get; set; }
    public string ToUserId { get; set; }
    public string MessageText { get; set; }
    public DateTime Timestamp { get; set; }
}
