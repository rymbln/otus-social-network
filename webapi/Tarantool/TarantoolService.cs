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

namespace OtusSocialNetwork.Tarantool;

public class TarantoolService: ITarantoolService, IDisposable
{
    private readonly string connStr;
    private readonly string spaceName = "";
    private Box box;
    private ISpace space;
    private ISchema schema;
    private IIndex _primaryIndex;
    private IIndex _secondaryUserIndex;
    private IIndex _secondaryUserDateIndex;

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
        this._secondaryUserDateIndex = await space.GetIndex("secondary_user_date");

    }

    public void Dispose()
    {
        box.Dispose();
    }

    public async Task WritePosts(string userId, List<PostDto> posts)
    {
        // Read existing posts
        var data = await _secondaryUserIndex.Select<TarantoolTuple<string>,
                   TarantoolTuple<string, string, long, string>>(
                   TarantoolTuple.Create(userId), new SelectOptions
                   {
                       Iterator = Iterator.All

                   });
        // Delete
        foreach (var item in data.Data)
        {
            await _primaryIndex.Delete<TarantoolTuple<string>, TarantoolTuple<string, string, long, string>>(
                TarantoolTuple.Create(item.Item1)
            );
        }

        // Write new posts
        var idx = 0;
        foreach (var post in posts.OrderByDescending(o => o.TimeStamp))
        {
            idx++;
            await space.Insert(TarantoolTuple.Create(
                Guid.NewGuid().ToString(),
                userId,
                idx,
                JsonSerializer.Serialize(post)));
        }
    }

    public async Task<List<PostDto>> ReadPosts(string userId)
    {
        var data = await _secondaryUserIndex.Select<TarantoolTuple<string>,
                    TarantoolTuple<string, string, long, string>>(
                    TarantoolTuple.Create(userId), new SelectOptions
                    {
                        Iterator = Iterator.All
                         
                    });
        var res = new List<PostDto>();
        foreach (var item in data.Data.OrderBy(o => o.Item3))
        {
            if (!string.IsNullOrEmpty(item.Item4))
            res.Add(JsonSerializer.Deserialize<PostDto>(item.Item4));
        }
        return res;
    }

    public async Task WritePost(string userId, PostDto post)
    {
        // Find old 1000 post and delete them
        var oldPost =  await _secondaryUserDateIndex.Select<
            TarantoolTuple<string, int>,
                   TarantoolTuple<string, string, long, string>>(
                   TarantoolTuple.Create(userId, 1000));
        foreach (var item in oldPost.Data)
        {
            await _primaryIndex.Delete<TarantoolTuple<string>, TarantoolTuple<string, string, long, string>>(TarantoolTuple.Create(item.Item1));
        }


       // Write new post
        await space.Insert(TarantoolTuple.Create(
            Guid.NewGuid().ToString(), userId, 0,JsonSerializer.Serialize(post)));

        // Re-enumerate posts
        await box.Call("update_post_idx", TarantoolTuple.Create(userId));
      

    }
}

