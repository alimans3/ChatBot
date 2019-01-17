using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace EDMChatBot.Core.Dialogs
{
    public interface IProductDialog:IDialog
    {
        Task<DialogTurnResult> ChooseItemCategoryStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
        
        Task<DialogTurnResult> ChooseItemStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<DialogTurnResult> ChooseQuantityStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<DialogTurnResult> ConfirmQuantityStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<DialogTurnResult> ToAnotherProductStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<bool> QuantityChoiceValidatorAsync(PromptValidatorContext<int> promptContext,
            CancellationToken cancellationToken);

        Task<bool> ProductChoiceValidatorAsync(PromptValidatorContext<FoundChoice> promptContext,
            CancellationToken cancellationToken);
        
        Task<bool> ItemCategoryChoiceValidatorAsync(PromptValidatorContext<FoundChoice> promptContext,
            CancellationToken cancellationToken);
    }
}