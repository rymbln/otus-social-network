using System;
namespace OtusSocialNetwork.Database.Entities;

public class PostEntity
{
    public PostEntity()
    {
    }

    public PostEntity(string authorUserId, string text)
    {
        AuthorUserId = authorUserId;
        Text = text;
    }

    public string Id { get; set; }
    public string AuthorUserId { get; set; }
    public string Text { get; set; }
    public DateTime TimeStamp { get; set; }
}

public class PostView
{
    public string PostId { get; set; }
    public string PostText { get; set; }
    public DateTime Timestamp { get; set; }
    public string FriendId { get; set; }
    public string FriendName { get; set; }

}
