using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace EDMChatBot.Core.Dialogs
{
    public interface ISalesOrderPostDialog : IDialog
    {
        Task<DialogTurnResult> ConfirmSalesOrderStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
        
        Task<DialogTurnResult> PostSalesOrderAndViewStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken);
    }
}