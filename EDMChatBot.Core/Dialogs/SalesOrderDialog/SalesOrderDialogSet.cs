using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace EDMChatBot.Core
{
    public class SalesOrderDialogSet : IOrderDialogSet
    {
        private readonly SalesDialogBotAccessors Accessors;    
        private readonly DialogSet salesSet;

        public SalesOrderDialogSet(SalesDialogBotAccessors accessors, ICustomerIdDialog customerIdDialog,
            IProductDialog productDialog, IAnotherProductDialog anotherProductDialog,
            ISalesOrderPostDialog salesOrderPostDialog, IVoiceVerificationDialog voiceVerificationDialog)
        {
            Accessors = accessors;
            salesSet = new DialogSet(accessors.OrderDialogState);

            var prompts = new List<Dialog>();
            prompts.AddRange(customerIdDialog.GetPrompts());
            prompts.AddRange(productDialog.GetPrompts());
            prompts.AddRange(anotherProductDialog.GetPrompts());
            prompts.AddRange(salesOrderPostDialog.GetPrompts());
            prompts.AddRange(voiceVerificationDialog.GetPrompts());

            salesSet.Add(new WaterfallDialog(customerIdDialog.GetName(), customerIdDialog.GetWaterfallSteps()));
            salesSet.Add(new WaterfallDialog(voiceVerificationDialog.GetName(), voiceVerificationDialog.GetWaterfallSteps()));
            salesSet.Add(new WaterfallDialog(productDialog.GetName(), productDialog.GetWaterfallSteps()));
            salesSet.Add(new WaterfallDialog(anotherProductDialog.GetName(), anotherProductDialog.GetWaterfallSteps()));
            salesSet.Add(new WaterfallDialog(salesOrderPostDialog.GetName(), salesOrderPostDialog.GetWaterfallSteps()));
            foreach (var dialog in prompts)
            {
                salesSet.Add(dialog);
            }
        }

        public async Task StartOrContinueDialogAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await salesSet.CreateContextAsync(turnContext, cancellationToken);
            if (dialogContext.ActiveDialog is null)
            {
                await ResetCustomerData(turnContext, cancellationToken,true, StaticTexts.StartOrderIntention);
                await dialogContext.BeginDialogAsync(CustomerIdDialog.Name, null, cancellationToken);
            }
            else
            {
                var context = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (context.Status == DialogTurnStatus.Complete)
                {
                    await ResetCustomerData(turnContext, cancellationToken,false, null);
                }
            }
        }
        
        public async Task EndDialogAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await salesSet.CreateContextAsync(turnContext, cancellationToken);
            await dialogContext.CancelAllDialogsAsync(cancellationToken);
            await ResetCustomerData(turnContext,cancellationToken,false, null);

        }

        private async Task ResetCustomerData(ITurnContext turnContext, CancellationToken cancellationToken,
            bool isOrdering, string intention)
        {
            var state = await Accessors.CustomerDataState.GetAsync(turnContext, cancellationToken: cancellationToken);
            state = StaticTexts.SetCustomerData(isOrdering, state.ChoosenLanguage, state.BotWelcomedUser,
                state.Customer, intention,false);
            await Accessors.CustomerDataState.SetAsync(turnContext, state, cancellationToken);
        }
    }
}