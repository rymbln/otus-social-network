using System;
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

    Task AddUserChatSocket(string userId, string connectionId);
    Task DeleteUserChatSocket(string connectionId);
    Task<List<ChatSocket>> GetUserChatSockets(string userId);

    Task<ChatItem> GetChat(string chatId);
    Task<List<ChatItem>> GetChats(string userId);
    Task CreateChat(string id, string name, List<string> userIds);
    Task DeleteChat(string id);

    Task AddMessage(string id, string chatId, string userId, string message);
    Task DeleteMessage(string id);
    Task<List<ChatMessageItem>> GetMessages(string chatId);

    Task<List<string>> GetConnectedUsers(List<string> userIds);


    Task SendDialogMessage(string fromId, string toId, string message);
    Task<List<DialogMessageDTO>> GetDialogMessages(string fromId, string toId);

}

