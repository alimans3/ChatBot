using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.AI;
using EDMChatBot.Core.Dialogs.ViewSalesOrderDialogs;
using EDMChatBot.NAVClient;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace EDMChatBot.Core.Dialogs
{
    public class CustomerIdDialogWithVoiceRecognition : ICustomerIdDialog
    {
       
        public static readonly string ConfirmIdPrompt = nameof(ConfirmIdPrompt);
        public static readonly string CustomerIdPrompt = nameof(CustomerIdPrompt);
        private WaterfallStep[] WaterfallSteps;
        public static string Name = nameof(CustomerIdDialog);
        private SalesDialogBotAccessors Accessors;
        private List<Dialog> Prompts;
        private readonly INavClient Client;
        private IChatter Chatter;


        public CustomerIdDialogWithVoiceRecognition(INavClient client, SalesDialogBotAccessors accessors,
            IChatter chatter)
        {
            Chatter = chatter;
            Client = client;
            Accessors = accessors;
            WaterfallSteps = new WaterfallStep[]
            {
                CustomerIdStepAsync,
                CustomerAuthenticationStepAsync,
                ConfirmProfileStepAsync,
                ToIntentionStepAsync
            };

            Prompts = new List<Dialog>
            {
                new TextPrompt(CustomerIdPrompt, CustomerIdValidatorAsync),
                new ConfirmPrompt(ConfirmIdPrompt)
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

        public async Task<DialogTurnResult> CustomerIdStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            if (state.Customer != null)
            {
                return await stepContext.ContinueDialogAsync(cancellationToken);
            }
            await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.WelcomeOrderText);
            return await stepContext.PromptAsync(CustomerIdPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.CustomerIdText),
                RetryPrompt =await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.CustomerNotFoundText)
            },cancellationToken);
        }

        public async Task<DialogTurnResult> CustomerAuthenticationStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            if (state.Customer != null)
            {
                return await stepContext.ContinueDialogAsync(cancellationToken);
            }

            return await stepContext.BeginDialogAsync(VoiceVerificationDialog.Name, null, cancellationToken);
        }

        public async Task<DialogTurnResult> ConfirmProfileStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var data = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            var attachment = StaticTexts.CreateCustomerAttachment(data.Customer);
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            return await stepContext.PromptAsync(ConfirmIdPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.ConfirmProfileText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.ConfirmProfileText)
            },cancellationToken);
        }

        public async Task<DialogTurnResult> ToIntentionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            var result = (bool) stepContext.Result;
            if (result)
            {
                if (state.CurrentIntention == StaticTexts.StartOrderIntention)
                {
                    return await stepContext.BeginDialogAsync(ProductDialog.Name,
                        cancellationToken: cancellationToken);
                }

                return await stepContext.BeginDialogAsync(ViewOrdersDialog.Name,
                    cancellationToken: cancellationToken);
            }
            var data = await Accessors.CustomerDataState.GetAsync(stepContext.Context,cancellationToken: cancellationToken);
            data.Customer = null;
            await Accessors.CustomerDataState.SetAsync(stepContext.Context, data, cancellationToken);
            return await stepContext.BeginDialogAsync(Name,
                cancellationToken: cancellationToken);
        }
        
        public async Task<bool> CustomerIdValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var customerDto = await Client.GetCustomer(promptContext.Context.Activity.Text);
                var data = await Accessors.CustomerDataState.GetAsync(promptContext.Context,cancellationToken: cancellationToken);
                data.UnAuthorizedCustomer = customerDto;
                await Accessors.CustomerDataState.SetAsync(promptContext.Context, data, cancellationToken);
                return true;
            }
            catch (NavClientException)
            {
                return false;
            }
        }
    }
}