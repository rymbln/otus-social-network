using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Options;

using OtusClasses.DataClasses;
using OtusClasses.DataClasses.Dtos;
using OtusClasses.Settings;

using ProGaudi.Tarantool.Client;
using ProGaudi.Tarantool.Client.Model;

using Rebus.Messages;

namespace OtusChatCounters.Database;

public interface ITarantoolService
{
    Task<List<ChatCounterDto>> SetUnreadsCount(string fromId, string toId);
    Task<List<ChatCounterDto>> GetUnreadsCount(string toId);
}

public class TarantoolService: ITarantoolService, IDisposable
{
    private readonly string connStr;
    private readonly ILogger<TarantoolService> _logger;
    private Box box;

    public TarantoolService(IOptions<TarantoolSettings> settings, ILogger<TarantoolService> logger)
    {
        _logger = logger;
        connStr = settings.Value.ConnStr;
        Init().Wait();

    }
    public void Dispose()
    {
        box.Dispose();
    }

    public async Task<List<ChatCounterDto>> GetUnreadsCount(string toId)
    {
        var res = new List<ChatCounterDto>();
        try
        {
            var data = await box.Call<
                TarantoolTuple<string>,
                TarantoolTuple<string, string, string, int>[]>(
                "get_counters_all", TarantoolTuple.Create(toId));
            foreach (var item in data.Data)
            {
                foreach (var el in item)
                {
                    var obj = new ChatCounterDto(el.Item2, el.Item3, el.Item4);
                    res.Add(obj);
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error");
        }
        return res;
    }

    public async Task<List<ChatCounterDto>> SetUnreadsCount(string fromId, string toId)
    {
        var res = new List<ChatCounterDto>();
        try
        {
            await box.Call("set_dialog_message_unread_count", TarantoolTuple.Create(fromId, toId));
            var data = await box.Call<
                TarantoolTuple<string>,
                TarantoolTuple<string, string, string, int>[]>(
                "get_counters_all", TarantoolTuple.Create(toId));
            foreach (var item in data.Data)
            {
                foreach (var el in item)
                {
                    var obj = new ChatCounterDto(el.Item2, el.Item3, el.Item4);
                    res.Add(obj);
                }

            }
        } catch (Exception ex)
        {
            _logger.LogError(ex, "Error");
        }
        return res;

    }

    [Obsolete]
    private async Task Init()
    {
        this.box = await Box.Connect(this.connStr);

    }

}
