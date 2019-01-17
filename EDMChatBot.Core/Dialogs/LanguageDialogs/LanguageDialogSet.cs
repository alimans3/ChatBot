using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace EDMChatBot.Core.Dialogs.LanguageDialogs
{
    public class LanguageDialogSet : ILanguageDialogSet
    {
        private readonly SalesDialogBotAccessors Accessors;
        private readonly DialogSet languageSet;
        private IChatter Chatter;
        
        public LanguageDialogSet(SalesDialogBotAccessors accessors, ILanguageDialog languageDialog,
             IChatter chatter)
        {
            Chatter = chatter;
            Accessors = accessors;
            languageSet = new DialogSet(accessors.LanguageDialogState);

            var prompts = new List<Dialog>();
            prompts.AddRange(languageDialog.GetPrompts());

            languageSet.Add(new WaterfallDialog(languageDialog.GetName(), languageDialog.GetWaterfallSteps()));
            foreach (var dialog in prompts)
            {
                languageSet.Add(dialog);
            }
        }

        public async Task StartOrContinueDialogAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await languageSet.CreateContextAsync(turnContext, cancellationToken);
            if (dialogContext.ActiveDialog is null)
            {
                await dialogContext.BeginDialogAsync(LanguageDialog.Name, null, cancellationToken);
            }
            else
            {
                var context = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (context.Status == DialogTurnStatus.Complete)
                {
                    await Chatter.SendMessageAsync(turnContext, StaticTexts.WelcomeText);
                }
            }
        }

        public async Task EndDialogAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await languageSet.CreateContextAsync(turnContext, cancellationToken);
            await dialogContext.CancelAllDialogsAsync(cancellationToken);
            await ResetCustomerData(turnContext, cancellationToken, false);
        }

        private async Task ResetCustomerData(ITurnContext turnContext, CancellationToken cancellationToken,
            bool isOrdering)
        {
            var state = await Accessors.CustomerDataState.GetAsync(turnContext,cancellationToken: cancellationToken);
            state = StaticTexts.SetCustomerData(isOrdering, state.ChoosenLanguage, state.BotWelcomedUser,
                state.Customer, null,false);
            await Accessors.CustomerDataState.SetAsync(turnContext, state, cancellationToken);
        }
    }

}