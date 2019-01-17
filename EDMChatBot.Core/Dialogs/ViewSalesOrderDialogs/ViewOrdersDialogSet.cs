using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace EDMChatBot.Core.Dialogs.ViewSalesOrderDialogs
{
    public class ViewOrdersDialogSet : IViewOrdersDialogSet
    {
        private readonly SalesDialogBotAccessors Accessors;
        private readonly DialogSet viewOrdersSet;
        private IChatter Chatter;
        
        public ViewOrdersDialogSet(SalesDialogBotAccessors accessors, IViewOrdersDialog viewOrdersDialog,
             ICustomerIdDialog customerIdDialog, IVoiceVerificationDialog voiceVerificationDialog,IChatter chatter)
        {
            Chatter = chatter;
            Accessors = accessors;
            viewOrdersSet = new DialogSet(accessors.ViewOrdersDialogState);

            var prompts = new List<Dialog>();
            prompts.AddRange(viewOrdersDialog.GetPrompts());
            prompts.AddRange(customerIdDialog.GetPrompts());
            prompts.AddRange(voiceVerificationDialog.GetPrompts());

            viewOrdersSet.Add(new WaterfallDialog(viewOrdersDialog.GetName(), viewOrdersDialog.GetWaterfallSteps()));
            viewOrdersSet.Add(new WaterfallDialog(voiceVerificationDialog.GetName(), voiceVerificationDialog.GetWaterfallSteps()));
            viewOrdersSet.Add(new WaterfallDialog(customerIdDialog.GetName(), customerIdDialog.GetWaterfallSteps()));
            foreach (var dialog in prompts)
            {
                viewOrdersSet.Add(dialog);
            }
        }

        public async Task StartOrContinueDialogAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await viewOrdersSet.CreateContextAsync(turnContext, cancellationToken);
            if (dialogContext.ActiveDialog is null)
            {
                await ResetCustomerData(turnContext, cancellationToken, false, StaticTexts.ViewOrdersIntention,true);
                await dialogContext.BeginDialogAsync(CustomerIdDialog.Name, null, cancellationToken);
            }
            else
            {
                var context = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (context.Status == DialogTurnStatus.Complete)
                {
                    await ResetCustomerData(turnContext, cancellationToken, false, StaticTexts.ViewOrdersIntention,
                        false);
                    await Chatter.SendMessageAsync(turnContext, StaticTexts.AnythingElseText);
                }
            }
        }

        public async Task EndDialogAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await viewOrdersSet.CreateContextAsync(turnContext, cancellationToken);
            await dialogContext.CancelAllDialogsAsync(cancellationToken);
            await ResetCustomerData(turnContext, cancellationToken, false,null,false);
        }

        private async Task ResetCustomerData(ITurnContext turnContext, CancellationToken cancellationToken,
            bool isOrdering, string intention, bool isViewing)
        {
            var state = await Accessors.CustomerDataState.GetAsync(turnContext,cancellationToken: cancellationToken);
            state = StaticTexts.SetCustomerData(isOrdering, state.ChoosenLanguage, state.BotWelcomedUser,
                state.Customer,intention,isViewing);
            await Accessors.CustomerDataState.SetAsync(turnContext, state, cancellationToken);
        }
    
    }
}