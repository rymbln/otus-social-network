﻿using OtusClasses.DataClasses.Dtos;

using OtusSocialNetwork.Database.Entities;
using OtusSocialNetwork.DataClasses.Dtos;

namespace OtusSocialNetwork.Database;


public interface IDatabaseContext
{
    Task<(bool isSuccess, string msg, AccountEntity? account)> GetLoginAsync(string id);
    Task<(bool isSuccess, string msg, string userId)> RegisterAsync(UserEntity user, string password);
    Task<(bool isSuccess, string msg, UserEntity? user)> GetUserAsync(string id);

    Task<(bool isSuccess, string msg, List<UserEntity> users)> SearchUserAsync(string firstName, string lastName);

    Task<(bool isSuccess, string msg, string userId)> AddNewTableRecordAsync(NewTableEntity user);

    Task<(bool isSuccess, string msg)> DeletePost(string id, string userId);
    Task<(bool isSuccess, string msg)> UpdatePost(string id, string userId, string text);
    Task<(bool isSuccess, string msg)> CreatePost(string text, string userId);
    Task<(bool isSuccess, string msg, List<PostEntity> posts)> GetPosts(string userId);
    Task<(bool isSuccess, string msg, PostEntity post)> GetPost(string id, string userId);
    Task<(bool isSuccess, string msg, PostView post)> GetPost(string id);
    Task<(bool isSuccess, string msg, List<PostView> posts)> GetFeed(string userId, int limit);

    Task<(bool isSuccess, string msg, List<FriendView> data)> GetFriends(string userId);
    Task<(bool isSuccess, string msg, List<FriendView> data)> SearchFriends(string query);
    Task<(bool isSuccess, string msg)> DeleteFriend(string userId, string friendId);
    Task<(bool isSuccess, string msg)> AddFriend(string userId, string friendId);

    Task<(bool isSuccess, string msg, ChatView chat)> GetChat(string chatId, string userId);
    Task<(bool isSuccess, string msg, List<ChatView> chats)> GetChats(string userId);
    Task<(bool isSuccess, string msg)> CreateChat(string ownerId, string userId);
    Task<(bool isSuccess, string msg)> DeleteChat(string chatId, string userId);
    Task<(bool isSuccess, string msg, List<ChatMessageView> messages)> GetMessages(string userId, string chatId);
    Task<(bool isSuccess, string msg)> CreateMessage(string chatId, string userId, string message);

    Task<(bool isSuccess, string msg)> DeleteMessage(string chatId, string userId, string messageId);


    Task<(bool isSuccess, string msg)> SendDialogMessage(string fromId, string toId, string message);
    Task<(bool isSuccess, string msg, List<DialogMessageDTO> messages)> GetDialogMessages(string fromId, string toId);

}
