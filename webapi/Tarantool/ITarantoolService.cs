﻿using System;
using OtusSocialNetwork.DataClasses.Dtos;

namespace OtusSocialNetwork.Tarantool;

public interface ITarantoolService
{
    Task WritePosts(string userId, List<PostDto> posts);
    Task WritePost(string userId, PostDto post);
    Task UpdatePost(PostDto post);
    Task DeletePost(string postId);
    Task<List<PostDto>> ReadPosts(string userId);
    Task AddUserSocket(string userId, string connectionId);
    Task DeleteUserSocket(string userId, string connectionId);
    Task<List<string>> GetConnectedUsers(List<string> userIds);
}

