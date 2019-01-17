using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace EDMChatBot.Core.Dialogs.ViewSalesOrderDialogs
{
    public interface IViewOrdersDialog : IDialog
    {
        Task<DialogTurnResult> ChooseOrderStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
        
        Task<DialogTurnResult> ChooseActionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
        
        Task<DialogTurnResult> ToActionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        Task<DialogTurnResult> AnotherActionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
        
        Task<bool> OrderNumberValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken);
    }
}