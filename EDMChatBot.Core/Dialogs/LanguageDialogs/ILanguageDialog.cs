using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace EDMChatBot.Core.Dialogs
{
    public interface ILanguageDialog : IDialog
    {
        Task<DialogTurnResult> LanguageChooseStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
        
        Task<DialogTurnResult> ToCustomerDialogStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<bool> LanguageValidatorAsync(PromptValidatorContext<FoundChoice> promptContext,
            CancellationToken cancellationToken);
    }
}