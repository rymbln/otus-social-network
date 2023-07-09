using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OtusSocialNetwork.DataClasses.Dtos;
using ProGaudi.Tarantool.Client;
using ProGaudi.Tarantool.Client.Model;
using ProGaudi.Tarantool.Client.Model.Enums;

using ProGaudi.Tarantool.Client.Model.Requests;
using ProGaudi.Tarantool.Client.Model.Enums;
using ProGaudi.Tarantool.Client.Model.Headers;
using ProGaudi.Tarantool.Client.Model.UpdateOperations;
using System.Reflection;
using ProGaudi.Tarantool.Client.Model.Responses;

namespace OtusSocialNetwork.Tarantool;

public class TarantoolService: ITarantoolService, IDisposable
{
    private readonly string connStr;
    private readonly string spaceName = "";
    private Box box;
    private ISpace space;
    private ISpace usersockets;
    private ISchema schema;
    private IIndex _primaryIndex;
    private IIndex _secondaryUserIndex;
    private IIndex _secondaryPostIndex;
    private IIndex _secondaryUserNumberIndex;
    private IIndex _signalUserIdx;
    private IIndex _signalConnectionIdx;

    public TarantoolService(IOptions<TarantoolSettings> settings)
	{
        connStr = settings.Value.ConnStr;
        spaceName = settings.Value.Space;
        Init().Wait();
        
    }

    [Obsolete]
    private async Task Init()
    {
        this.box = await Box.Connect(this.connStr);
        this.schema = box.GetSchema();
        this.space = await schema.GetSpace(this.spaceName);

        this._primaryIndex = await space.GetIndex("primary");
        this._secondaryUserIndex = await space.GetIndex("secondary_user");
        this._secondaryPostIndex = await space.GetIndex("secondary_post");
        this._secondaryUserNumberIndex = await space.GetIndex("secondary_user_number");

        this.usersockets = await schema.GetSpace("usersockets");
        _signalUserIdx = await usersockets.GetIndex("usersockets_idx_userid");
        _signalConnectionIdx = await usersockets.GetIndex("usersockets_idx_connectionid");
    }

    public void Dispose()
    {
        box.Dispose();
    }

    public async Task WritePosts(string userId, List<PostDto> posts)
    {
        //// Read existing posts
        //var data = await _secondaryUserIndex.Select<TarantoolTuple<string>,
        //           TarantoolTuple<string, string, long, string>>(
        //           TarantoolTuple.Create(userId), new SelectOptions
        //           {
        //               Iterator = Iterator.All

        //           });
        //// Delete
        //foreach (var item in data.Data)
        //{
        //    await _primaryIndex.Delete<TarantoolTuple<string>, TarantoolTuple<string, string, long, string>>(
        //        TarantoolTuple.Create(item.Item1)
        //    );
        //}

        // Delete user posts
        await box.Call("delete_user_posts", TarantoolTuple.Create(userId));

        // Write new posts
        var idx = 0;
        foreach (var post in posts.OrderByDescending(o => o.TimeStamp))
        {
            idx++;
            await space.Insert(TarantoolTuple.Create(
                Guid.NewGuid().ToString(),
                userId,
                post.Id,
                idx,
                JsonSerializer.Serialize(post)));
        }
    }

    public async Task<List<PostDto>> ReadPosts(string userId)
    {
        var data = await _secondaryUserIndex.Select<TarantoolTuple<string>,
                    TarantoolTuple<string, string, string, long, string>>(
                    TarantoolTuple.Create(userId), new SelectOptions
                    {
                        Iterator = Iterator.All
                                         });
        var res = new List<PostDto>();
        foreach (var item in data.Data.OrderBy(o => o.Item3))
        {
            if (!string.IsNullOrEmpty(item.Item5))
            res.Add(JsonSerializer.Deserialize<PostDto>(item.Item5));
        }
        return res;
    }

    public async Task WritePost(string userId, PostDto post)
    {
        // Find old 1000 post and delete them
        var oldPost =  await _secondaryUserNumberIndex.Select<
            TarantoolTuple<string, int>,
                   TarantoolTuple<string, string, string, long, string>>(
                   TarantoolTuple.Create(userId, 1000));
        foreach (var item in oldPost.Data)
        {
            await _primaryIndex.Delete<
                TarantoolTuple<string>,
                TarantoolTuple<string, string, string, long, string>
                >(TarantoolTuple.Create(item.Item1));
        }


        // Write new post

        await space.Insert(TarantoolTuple.Create(Guid.NewGuid().ToString(), userId, post.Id, (long)0,JsonSerializer.Serialize(post)));

        // Re-enumerate posts
        await box.Call("update_post_idx", TarantoolTuple.Create(userId));
      

    }

    public async Task UpdatePost(PostDto post)
    {
        var oldPost = await _secondaryPostIndex.Select<
        TarantoolTuple<string>,
               TarantoolTuple<string, string, string, long, string>>(
               TarantoolTuple.Create(post.Id));

        foreach (var item in oldPost.Data)
        {
            var updatedData = await space.Update<
                    TarantoolTuple<string>,
                    TarantoolTuple<string, string, string, long, string>>(
                TarantoolTuple.Create(item.Item1),
                new UpdateOperation[]
                {
                    UpdateOperation.CreateAssign(5, JsonSerializer.Serialize(post))
                }
                );
        }
    }

    public async Task DeletePost(string postid)
    {
        await box.Call("delete_post", TarantoolTuple.Create(postid));
    }

    public async Task AddUserSocket(string userId, string connectionId)
    {
        await usersockets.Insert(TarantoolTuple.Create(Guid.NewGuid().ToString(), userId, connectionId));
    }

    public async Task DeleteUserSocket(string userId, string connectionId)
    {
        DataResponse<TarantoolTuple<string, string, string>[]> data;
        if (!string.IsNullOrEmpty(userId))
        {
            data = await _signalUserIdx.Select<TarantoolTuple<string>, TarantoolTuple<string, string, string>>(TarantoolTuple.Create(userId));
            foreach (var item in data.Data)
            {
                await _primaryIndex.Delete<TarantoolTuple<string>, TarantoolTuple<string, string, string>>(TarantoolTuple.Create(item.Item1));
            }
            //await box.Call("delete_usersockets_by_user", TarantoolTuple.Create(userId));
        } else if (!string.IsNullOrEmpty(connectionId))
        {
            data = await _signalConnectionIdx.Select<TarantoolTuple<string>, TarantoolTuple<string, string, string>>(TarantoolTuple.Create(connectionId));
            foreach (var item in data.Data)
            {
                await _primaryIndex.Delete<TarantoolTuple<string>, TarantoolTuple<string, string, string>>(TarantoolTuple.Create(item.Item1));
            }
            //await box.Call("delete_usersockets_by_connection", TarantoolTuple.Create(connectionId));
        }

    }

    public async Task<List<string>> GetConnectedUsers(List<string> userIds)
    {
        var connections = new List<string>();

        foreach (var userId in userIds)
        {
            var data = await _signalUserIdx.Select<TarantoolTuple<string>,
                    TarantoolTuple<string, string, string>>(
                    TarantoolTuple.Create(userId), new SelectOptions
                    {
                        Iterator = Iterator.All
                    });
            foreach (var item in data.Data)
            {
                connections.Add(item.Item3);
            }
        }
        return connections;
    }
}

