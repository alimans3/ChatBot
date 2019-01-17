using System.Threading.Tasks;

namespace EDMChatBot.Core.AI
{
    public interface ISpeechRecognizer
    {
        Task<string> RecognizeSpeech(string audioUrl);
    }
}