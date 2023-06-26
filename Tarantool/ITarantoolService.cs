using System;
using OtusSocialNetwork.DataClasses.Dtos;

namespace OtusSocialNetwork.Tarantool;

public interface ITarantoolService
{
    Task WritePosts(string userId, List<PostDto> posts);
    Task ReadPosts(string userId);
}

