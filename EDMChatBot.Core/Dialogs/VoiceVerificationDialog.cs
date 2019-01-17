using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.AI;
using EDMChatBot.NAVClient;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace EDMChatBot.Core.Dialogs
{
    public class VoiceVerificationDialog : IVoiceVerificationDialog
    {
        public static readonly string PhrasePrompt = nameof(PhrasePrompt);
        public static readonly string VoicePrompt = nameof(VoicePrompt);
        private WaterfallStep[] WaterfallSteps;
        public static string Name = nameof(VoiceVerificationDialog);
        private SalesDialogBotAccessors Accessors;
        private List<Dialog> Prompts;
        private IChatter Chatter;
        private IVoiceVerifier VoiceVerifier;


        public VoiceVerificationDialog(SalesDialogBotAccessors accessors, IVoiceVerifier verifier,
            IChatter chatter)
        {
            VoiceVerifier = verifier;
            Chatter = chatter;
            Accessors = accessors;
            WaterfallSteps = new WaterfallStep[]
            {
                PhraseStepAsync,
                EndVoiceAsync
            };

            Prompts = new List<Dialog>
            {
                new AttachmentPrompt(PhrasePrompt,VoiceValidatorAsync),
                new AttachmentPrompt(VoicePrompt,VoiceAuthenticatorAsync)
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

        public async Task<DialogTurnResult> PhraseStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context, null, cancellationToken);
            var phrase = await VoiceVerifier.GetEnrollmentPhraseAsync();
            var profileEnrollments = await VoiceVerifier.GetNumberOfEnrollments(state.UnAuthorizedCustomer);
            if (profileEnrollments >= 3)
            {
                return await stepContext.PromptAsync(VoicePrompt, new PromptOptions
                {
                    Prompt = await Chatter.GetActivityAsIsAsync(StaticTexts.UploadVerificationVoiceText(phrase)),
                    RetryPrompt = await Chatter.GetActivityAsIsAsync(StaticTexts.VoiceNotAuthenticText(phrase))
                }, cancellationToken);
            }

            await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.UploadVoicesIntroText);
            return await stepContext.PromptAsync(PhrasePrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsIsAsync(
                    StaticTexts.UploadVoiceText(phrase, profileEnrollments + 1))
            }, cancellationToken);

        }

        public async Task<DialogTurnResult> EndVoiceAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context, null, cancellationToken); 
            var profileEnrollments = await VoiceVerifier.GetNumberOfEnrollments(state.UnAuthorizedCustomer);
            if (state.Customer != null)
            {
                await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.AuthorizedText);
                return await stepContext.EndDialogAsync(null,cancellationToken);
            }
            else if (profileEnrollments == 3 && state.Customer == null)
            {
                await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.VoicesAddedText);
                state.Customer = state.UnAuthorizedCustomer;
                await Accessors.CustomerDataState.SetAsync(stepContext.Context, state, cancellationToken);
                return await stepContext.EndDialogAsync(null,cancellationToken);
            }
            return await stepContext.BeginDialogAsync(Name, null, cancellationToken);
        }

        public async Task<bool> VoiceValidatorAsync(PromptValidatorContext<IList<Attachment>> promptContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(promptContext.Context, null, cancellationToken);
            var phrase = await VoiceVerifier.GetEnrollmentPhraseAsync();
            var profileEnrollments = await VoiceVerifier.GetNumberOfEnrollments(state.UnAuthorizedCustomer);
            try
            {
                if (promptContext.Recognized.Succeeded)
                {
                    if (!await VoiceVerifier.CheckIfCustomerHasProfileAsync(state.UnAuthorizedCustomer))
                    {
                        await VoiceVerifier.AddCustomerProfileAsync(state.UnAuthorizedCustomer);
                    }

                    await VoiceVerifier.AddCustomerEnrollmentAsync(state.UnAuthorizedCustomer,
                        promptContext.Recognized.Value[0].ContentUrl);
                    return true;
                }

                await Chatter.SendMessageAsIsAsync(promptContext.Context,
                    StaticTexts.UploadVoiceText(phrase, profileEnrollments + 1));
                return false;
            }
            catch (Exception)
            {
                await Chatter.SendMessageAsIsAsync(promptContext.Context,
                    StaticTexts.UploadVoiceText(phrase, profileEnrollments + 1));
                return false;
            }

        }

        public async Task<bool> VoiceAuthenticatorAsync(PromptValidatorContext<IList<Attachment>> promptContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(promptContext.Context, null, cancellationToken);
            var phrase = await VoiceVerifier.GetEnrollmentPhraseAsync();
            if (promptContext.Recognized.Succeeded)
            {
                var authorized = await VoiceVerifier.AuthorizeCustomerAsync(state.UnAuthorizedCustomer,
                    promptContext.Recognized.Value[0].ContentUrl);
                if (authorized)
                {
                    state.Customer = state.UnAuthorizedCustomer;
                    await Accessors.CustomerDataState.SetAsync(promptContext.Context, state, cancellationToken);
                }
                else
                {
                    await Chatter.SendMessageAsIsAsync(promptContext.Context,
                        StaticTexts.VoiceNotAuthenticText(phrase));
                }

                return authorized;
            }
            await Chatter.SendMessageAsIsAsync(promptContext.Context,
                StaticTexts.VoiceNotAuthenticText(phrase));
            return false;
        }
    }
}