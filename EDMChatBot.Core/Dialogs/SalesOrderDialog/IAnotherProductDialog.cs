using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace EDMChatBot.Core.Dialogs
{
    public interface IAnotherProductDialog : IDialog
    {
        Task<DialogTurnResult> AnotherProductOrNotStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<DialogTurnResult> ThankYouStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
    }
}