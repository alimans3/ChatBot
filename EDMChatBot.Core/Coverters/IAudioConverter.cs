using System.Threading.Tasks;

namespace EDMChatBot.Core.Coverters
{
    public interface IAudioConverter
    {
        Task<byte[]> ConvertOggToWav(string url);
    }
}