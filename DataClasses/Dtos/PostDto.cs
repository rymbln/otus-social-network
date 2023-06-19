using System;
namespace OtusSocialNetwork.DataClasses.Dtos;

public class PostDto
{
	public PostDto()
	{
	}

	public string Id { get; set; }
	public string AuthorUserId { get; set; }
	public string AuthorUserName { get; set; }
	public string Text { get; set; }
	public DateTime TimeStamp { get; set; }
}

