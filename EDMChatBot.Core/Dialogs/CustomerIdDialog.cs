using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.AI;
using EDMChatBot.Core.Dialogs.ViewSalesOrderDialogs;
using EDMChatBot.NAVClient;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace EDMChatBot.Core.Dialogs
{
    public class CustomerIdDialog : ICustomerIdDialog
    {
        public static readonly string ConfirmIdPrompt = nameof(ConfirmIdPrompt);
        public static readonly string CustomerIdPrompt = nameof(CustomerIdPrompt);
        public static readonly string VerifyByPicturePromt = nameof(VerifyByPicturePromt);
        private WaterfallStep[] WaterfallSteps;
        public static string Name = nameof(CustomerIdDialog);
        private SalesDialogBotAccessors Accessors;
        private List<Dialog> Prompts;
        private readonly INavClient Client;
        private readonly IFaceRecognizer FaceRecognizer;
        private IChatter Chatter;


        public CustomerIdDialog(INavClient client, SalesDialogBotAccessors accessors, IFaceRecognizer faceRecognizer,
            IChatter chatter)
        {
            Chatter = chatter;
            Client = client;
            Accessors = accessors;
            FaceRecognizer = faceRecognizer;
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
                new ConfirmPrompt(ConfirmIdPrompt),
                new AttachmentPrompt(VerifyByPicturePromt, AuthenticationValidatorAsync)
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
            return await stepContext.PromptAsync(VerifyByPicturePromt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.VerifyByPictureText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.VerifyByPictureText)
            },cancellationToken);
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

        public async Task<bool> AuthenticationValidatorAsync(PromptValidatorContext<IList<Attachment>> promptContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var state = await Accessors.CustomerDataState.GetAsync(promptContext.Context,
                    cancellationToken: cancellationToken);
                var picture = promptContext.Recognized.Value[0];
                var hasPicture =
                    await FaceRecognizer.CheckIfCustomerHasPersonAsync(state.UnAuthorizedCustomer, cancellationToken);
                if (hasPicture)
                {
                    var authorizedPicture =
                        await FaceRecognizer.AuthorizeCustomerAsync(state.UnAuthorizedCustomer, picture.ContentUrl,
                            cancellationToken);
                    if (authorizedPicture)
                    {
                        state.Customer = state.UnAuthorizedCustomer;
                        await Chatter.SendMessageAsync(promptContext.Context, StaticTexts.AuthorizedText);
                        return true;
                    }

                    await Chatter.SendMessageAsync(promptContext.Context, StaticTexts.PictureNotValidText);
                    return false;
                }

                await FaceRecognizer.AddCustomerWithPictureAsync(state.UnAuthorizedCustomer, picture.ContentUrl,
                    cancellationToken);


                await Chatter.SendMessageAsync(promptContext.Context, StaticTexts.PersonCreatedText);
                state.Customer = state.UnAuthorizedCustomer;
                await Accessors.CustomerDataState.SetAsync(promptContext.Context, state, cancellationToken);
                return true;
            }
            catch (FaceApiException)
            {
                await Chatter.SendMessageAsync(promptContext.Context, StaticTexts.PictureMoreOnePersonText);
                return false;
            }
        }
    }
}