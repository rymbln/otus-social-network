using System;
using OtusSocialNetwork.DataClasses.Dtos;

namespace OtusSocialNetwork.Tarantool;

public interface ITarantoolService
{
    Task WritePosts(string userId, List<PostDto> posts);
    Task WritePost(string userId, PostDto post);
    Task<List<PostDto>> ReadPosts(string userId);
}

