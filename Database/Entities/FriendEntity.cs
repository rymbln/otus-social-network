using System;
namespace OtusSocialNetwork.Database.Entities;

public class FriendEntity
{
	public FriendEntity()
	{
		
	}
    public string UserId { get; set; }
    public string FriendId { get; set; }
}

public class FriendView
{
	public string Id { get; set; }
	public string FullName { get; set; }
}

