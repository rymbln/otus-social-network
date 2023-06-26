using System;
using Microsoft.Extensions.Options;
using OtusSocialNetwork.DataClasses.Dtos;
using ProGaudi.Tarantool.Client;

namespace OtusSocialNetwork.Tarantool;

public class TarantoolService: ITarantoolService, IDisposable
{
    private readonly string connStr;
    private readonly string spaceName = "";
    private Box box;
    private ISpace space;
    private ISchema schema;
    private IIndex _primaryIndex;
    private IIndex _secondaryIndex;

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
        var schema = box.GetSchema();
        var space = await schema.GetSpace(this.spaceName);
        this._primaryIndex = await space.GetIndex("primary");
        this._secondaryIndex = await space.GetIndex("secondary");

    }

    public void Dispose()
    {
        box.Dispose();
    }

    public Task WritePosts(string userId, List<PostDto> posts)
    {
        throw new NotImplementedException();
    }

    public Task ReadPosts(string userId)
    {
        throw new NotImplementedException();
    }
}

