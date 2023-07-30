using OtusClasses.DataClasses.Dtos;

namespace OtusDialogsGrpc.Database.Interfaces
{
    public interface ITarantoolService
    {
        Task<DialogMessageDTO> SendDialogMessage(string fromId, string toId, string message);
        Task<List<DialogMessageDTO>> GetDialogMessages(string fromId, string toId);
    }
}
