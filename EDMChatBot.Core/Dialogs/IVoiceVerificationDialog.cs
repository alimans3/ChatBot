using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace EDMChatBot.Core.Dialogs
{
    public interface IVoiceVerificationDialog : IDialog
    {
        Task<DialogTurnResult> PhraseStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
        
        Task<DialogTurnResult> EndVoiceAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
        
        Task<bool> VoiceValidatorAsync(PromptValidatorContext<IList<Attachment>> promptContext,
            CancellationToken cancellationToken);

        Task<bool> VoiceAuthenticatorAsync(PromptValidatorContext<IList<Attachment>> promptContext,
            CancellationToken cancellationToken);

    }
}