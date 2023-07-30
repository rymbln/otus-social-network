using OtusClasses.DataClasses.Dtos;

namespace OtusSocialNetwork.Services
{
    public interface IDialogsService
    {
        Task<List<DialogMessageDTO>> GetDialog(string from, string to);
        Task<DialogMessageDTO> SendMessage(string from, string to, string message);
    }
}