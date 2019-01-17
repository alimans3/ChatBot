using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace EDMChatBot.Core.AI
{
    public interface INaturalLanguageEngine
    {
        Task<string> RecognizeIntention(string text, CancellationToken token);
    }
}