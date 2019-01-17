using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.AI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace EDMChatBot.Core.Dialogs
{
    public class AnotherProductDialog : IAnotherProductDialog
    {
        public static readonly string AnotherProductPrompt = nameof(AnotherProductPrompt);
        public static readonly string ConfirmOrderPrompt = nameof(ConfirmOrderPrompt);
        private WaterfallStep[] WaterfallSteps;
        public static string Name = nameof(AnotherProductDialog);
        private SalesDialogBotAccessors Accessors;
        private List<Dialog> Prompts;
        private IChatter Chatter;
        
        public AnotherProductDialog(SalesDialogBotAccessors accessors, IChatter chatter)
        {
            Chatter = chatter;
            Accessors = accessors;
            WaterfallSteps = new WaterfallStep[]
            {
                AnotherProductOrNotStepAsync,
                ThankYouStepAsync
            };
            Prompts = new List<Dialog>
            {
                new ConfirmPrompt(AnotherProductPrompt),
                new ConfirmPrompt(ConfirmOrderPrompt)
            };
        }
  
        public WaterfallStep[] GetWaterfallSteps()
        {
            return WaterfallSteps;
        }

        public string GetName()
        {
            return Name;
        }

        public List<Dialog> GetPrompts()
        {
            return Prompts;
        }


        public async Task<DialogTurnResult> AnotherProductOrNotStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            
            var salesOrderLine = new SalesOrderLine
            {
                Item = state.TempItem,
                Quantity = state.TempQuantity,
                UnitPrice = state.TempItem.UnitPrice
            };
            if (state.TempSalesOrderLines != null)
            {
                state.TempSalesOrderLines.Add(salesOrderLine);
            }
            else
            {
                state.TempSalesOrderLines = new List<SalesOrderLine>{salesOrderLine};
            }

            await Accessors.CustomerDataState.SetAsync(stepContext.Context, state, cancellationToken);
            return await stepContext.PromptAsync(AnotherProductPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.AnotherProductText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.AnotherProductText)
            }, cancellationToken);
        }

        public async Task<DialogTurnResult> ThankYouStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var confirm = (bool)stepContext.Result;
            if (confirm)
            {
                return await stepContext.BeginDialogAsync(ProductDialog.Name,cancellationToken: cancellationToken);
            }

            return await stepContext.BeginDialogAsync(SalesOrderPostDialog.Name,cancellationToken: cancellationToken);
        }
    }
}