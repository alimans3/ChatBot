using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace EDMChatBot.Core.Dialogs
{
    public interface ICustomerIdDialog:IDialog
    {
        Task<DialogTurnResult> CustomerIdStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
        
        Task<DialogTurnResult> CustomerAuthenticationStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<DialogTurnResult> ConfirmProfileStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<DialogTurnResult> ToIntentionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<bool> CustomerIdValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken);
    }
}