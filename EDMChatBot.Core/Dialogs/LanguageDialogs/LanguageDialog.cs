using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.AI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace EDMChatBot.Core.Dialogs
{
    public class LanguageDialog : ILanguageDialog
    {
        public static readonly string LanguagePrompt = nameof(LanguagePrompt);
        private WaterfallStep[] WaterfallSteps;
        public static string Name = nameof(LanguageDialog);
        private SalesDialogBotAccessors Accessors;
        private ITextTranslator Translator;
        private List<Dialog> Prompts;
        private IChatter Chatter;


        public LanguageDialog(SalesDialogBotAccessors accessors, ITextTranslator translator, IChatter chatter)
        {
            Chatter = chatter;
            Translator = translator;
            Accessors = accessors;
            WaterfallSteps = new WaterfallStep[]
            {
                LanguageChooseStepAsync,
                ToCustomerDialogStepAsync
            };
            Prompts = new List<Dialog>
            {
                new ChoicePrompt(LanguagePrompt, LanguageValidatorAsync)
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

        public async Task<DialogTurnResult> LanguageChooseStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var choices = await Translator.GetLanguages();
            return await stepContext.PromptAsync(LanguagePrompt, new PromptOptions
            {
                Choices = ChoiceFactory.ToChoices(choices),
                Prompt = await Chatter.GetActivityAsync(stepContext.Context, StaticTexts.ChooseLanguageText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context, StaticTexts.ChooseLanguageAgainText)
            }, cancellationToken);
        }

        public async Task<DialogTurnResult> ToCustomerDialogStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            var foundChoice = (FoundChoice) stepContext.Result;
            state.ChoosenLanguage = foundChoice.Value;
            await Accessors.CustomerDataState.SetAsync(stepContext.Context, state, cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        public Task<bool> LanguageValidatorAsync(PromptValidatorContext<FoundChoice> promptContext,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(promptContext.Recognized.Succeeded);
        }
    }
}