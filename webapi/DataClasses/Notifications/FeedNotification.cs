using System;
using OtusSocialNetwork.DataClasses.Dtos;

namespace OtusSocialNetwork.DataClasses.Notifications
{
    public interface INotificationChatSended
    {
        public string ChatId { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
    }
    public interface INotificationChatSaved
    {
        public string ChatId { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
    }
    public interface INotificationFeedReload
	{
		string UserId { get; set; }
	}
	public interface INotificationFeedAdd
	{
		List<string> UserIds { get; set; }
		string PostId { get; set; }
        PostDto Post { get; set; }
	}
    public interface INotificationFeedUpdate
    { 
        string PostId { get; set; }
        PostDto Post { get; set; }
    }
    public interface INotificationFeedDelete
    {
        string PostId { get; set; }
    }

    public class NotificationFeedReload : INotificationFeedReload
    {
        public NotificationFeedReload(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; set; }
    }

    public class NotificationFeedAdd: INotificationFeedAdd
    {
        public NotificationFeedAdd(List<string> userIds, string postId, PostDto post)
        {
            UserIds = userIds;
            PostId = postId;
            Post = post;
        }

        public List<string> UserIds { get; set; }
        public string PostId { get; set; }
        public PostDto Post { get; set; }
    }

    public class NotificationFeedUpdate: INotificationFeedUpdate
    {
        public NotificationFeedUpdate(string postId, PostDto post)
        {
            PostId = postId;
            Post = post;
        }

        public string PostId { get; set; }
        public PostDto Post { get; set; }
    }

    public class NotificationFeedDelete: INotificationFeedDelete
    {
        public NotificationFeedDelete(string postId)
        {
            PostId = postId;
        }

        public string PostId { get; set; }
    }
}

