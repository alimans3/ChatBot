using System.Threading.Tasks;
using EDMChatBot.Core.AI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EDMChatBot.Core
{
    public class ChatterWithTranslation : IChatter
    {
        private SalesDialogBotAccessors Accessors;
        private ITextTranslator Translator;
        
        public ChatterWithTranslation(SalesDialogBotAccessors accessors, ITextTranslator translator)
        {
            Translator = translator;
            Accessors = accessors;
        }
        
        public async Task SendMessageAsync(ITurnContext turnContext, string message)
        {
            var state = await Accessors.CustomerDataState.GetAsync(turnContext);
            if (state.ChoosenLanguage != null)
            {
                var text = await Translator.TranslateText(message, state.ChoosenLanguage);
                await turnContext.SendActivityAsync(MessageFactory.Text(text));
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(message));
            }
        }

        public async Task<Activity> GetActivityAsync(ITurnContext turnContext,string message)
        {
            var state = await Accessors.CustomerDataState.GetAsync(turnContext);
            if (state.ChoosenLanguage != null)
            {
                var text = await Translator.TranslateText(message, state.ChoosenLanguage);
                return MessageFactory.Text(text);
            }
            else
            {
                return MessageFactory.Text(message);
            }
        }

        public Task<Activity> GetActivityAsIsAsync(string message)
        {
            return Task.FromResult(MessageFactory.Text(message));
        }

        public async Task<string> GetInputAsync(ITurnContext turnContext)
        {
            var state = await Accessors.CustomerDataState.GetAsync(turnContext);
            if (state.ChoosenLanguage != "en")
            {
                return await Translator.TranslateText(turnContext.Activity.Text, "en");
            }
            else
            {
                return turnContext.Activity.Text;
            }
        }

        public async Task SendMessageAsIsAsync(ITurnContext turnContext, string message)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(message));
        }
    }
}