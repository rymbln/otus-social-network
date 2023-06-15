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

