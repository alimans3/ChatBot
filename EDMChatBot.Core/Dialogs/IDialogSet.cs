using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace EDMChatBot.Core
{
    public interface IDialogSet
    {
        Task StartOrContinueDialogAsync(ITurnContext turnContext, CancellationToken cancellationToken);
        
        Task EndDialogAsync(ITurnContext turnContext, CancellationToken cancellationToken);
    }
}