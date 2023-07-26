using Microsoft.Extensions.Options;

using OtusClasses.DataClasses.Dtos;
using OtusClasses.Settings;

using OtusDialogsGrpc.Database.Interfaces;

using ProGaudi.Tarantool.Client;
using ProGaudi.Tarantool.Client.Model;

namespace OtusDialogsGrpc.Database;

public class TarantoolDialogsService : ITarantoolDialogsService, IDisposable
{
    private readonly string connStr;
    private Box box;

    public TarantoolDialogsService(IOptions<TarantoolSettings> settings)
    {
        connStr = settings.Value.ConnStr;
        Init().Wait();

    }
    public void Dispose()
    {
        box.Dispose();
    }


    [Obsolete]
    private async Task Init()
    {
        this.box = await Box.Connect(this.connStr);
        
    }

    public async Task<DialogMessageDTO> SendDialogMessage(string fromId, string toId, string message)
    {
        var id = Guid.NewGuid().ToString();
        var timestamp = DateTime.Now.ToUniversalTime();
        await box.Call("create_dialog_message", TarantoolTuple.Create(id, fromId, toId, message, timestamp.ToString()));
        return new DialogMessageDTO(id, fromId, toId, message, timestamp);
    }

    public async Task<List<DialogMessageDTO>> GetDialogMessages(string fromId, string toId)
    {
        var res = new List<DialogMessageDTO>();
        try
        {

            var data = await box.Call<TarantoolTuple<string, string>,
                TarantoolTuple<string, string, string, string, string>[]>(
                 "get_messages_for_dialog",
                 TarantoolTuple.Create(fromId, toId)
                );
            foreach (var item in data.Data)
            {
                foreach (var el in item)
                {
                    var obj = new DialogMessageDTO(el.Item2, el.Item3, el.Item4, DateTimeOffset.Parse(el.Item5).UtcDateTime);
                    res.Add(obj);
                }

            }
            //return res;
        }
        catch (Exception ex)
        {
            //return res;
        }
        return res.OrderBy(o => o.TimeStamp).ToList();
    }
}
