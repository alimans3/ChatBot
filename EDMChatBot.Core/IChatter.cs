using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EDMChatBot.Core
{
    public interface IChatter
    {
        Task SendMessageAsync(ITurnContext turnContext, string message);

        Task<Activity> GetActivityAsync(ITurnContext turnContext, string message);
        
        Task<Activity> GetActivityAsIsAsync(string message);

        Task<string> GetInputAsync(ITurnContext turnContext);
        
        Task SendMessageAsIsAsync(ITurnContext turnContext, string message);
    }
}