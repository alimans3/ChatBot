// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using EDMChatBot.Core;
using EDMChatBot.Core.AI;
using EDMChatBot.Core.Dialogs;
using EDMChatBot.NAVClient;
using Newtonsoft.Json;

namespace EDMChatBot
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;

    public class ChatBot : IBot
    {
        private readonly ILogger<ChatBot> Logger;
        private readonly SalesDialogBotAccessors Accessors;
        private readonly IOrderDialogSet SalesDialogSet;
        private ILanguageDialogSet LanguageDialogSet;
        private INaturalLanguageEngine NaturalLanguageEngine;
        private IViewOrdersDialogSet ViewOrdersDialogSet;
        private IChatter Chatter;
        private ISpeechRecognizer SpeechRecognizer;

        public ChatBot(ILogger<ChatBot> logger, SalesDialogBotAccessors accessors, IOrderDialogSet salesDialogSet,
            INaturalLanguageEngine naturalLanguageEngine, IChatter chatter, ILanguageDialogSet languageDialogSet,
            IViewOrdersDialogSet viewOrdersDialogSet, ISpeechRecognizer speechRecognizer)
        {
            SpeechRecognizer = speechRecognizer;
            ViewOrdersDialogSet = viewOrdersDialogSet;
            LanguageDialogSet = languageDialogSet;
            Chatter = chatter;
            NaturalLanguageEngine = naturalLanguageEngine;
            Logger = logger;
            Accessors = accessors;
            SalesDialogSet = salesDialogSet;
        }

        public async Task OnTurnAsync(ITurnContext turnContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = await Accessors.CustomerDataState.GetAsync(turnContext,
                () => StaticTexts.SetCustomerData(false, null, false, null, null, false),
                cancellationToken: cancellationToken);
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext.Activity.Attachments != null)
                {
                    var url = turnContext.Activity.Attachments[0].ContentUrl;
                    var transcribed = await SpeechRecognizer.RecognizeSpeech(url);
                    if (transcribed != "")
                    {
                        await Chatter.SendMessageAsIsAsync(turnContext, "You said:" + transcribed);
                    }
                    turnContext.Activity.Text = transcribed;
                }

                var text = await Chatter.GetInputAsync(turnContext);
                var intention =
                    await NaturalLanguageEngine.RecognizeIntention(text, cancellationToken);
                bool ordering = state.isOrdering;
                bool viewing = state.isViewing;
                
                
                if (!state.BotWelcomedUser || state.ChoosenLanguage == null)
                {
                    await LanguageDialogSet.StartOrContinueDialogAsync(turnContext, cancellationToken);
                    state.BotWelcomedUser = true;
                    await Accessors.CustomerDataState.SetAsync(turnContext, state, cancellationToken);
                }
                else if (intention == StaticTexts.StartOverIntention)
                {
                    await EndAllDialogsAsync(turnContext, cancellationToken);
                    await Chatter.SendMessageAsync(turnContext, StaticTexts.CancelText);
                    await Chatter.SendMessageAsync(turnContext, StaticTexts.AnythingElseText);
                }
                else if (intention == StaticTexts.HelpIntention)
                {
                    await Chatter.SendMessageAsync(turnContext, StaticTexts.HelpText);
                }
                else if (intention == StaticTexts.ChangeLanguageIntention)
                {
                    await EndAllDialogsAsync(turnContext, cancellationToken);
                    state.ChoosenLanguage = null;
                    await Accessors.CustomerDataState.SetAsync(turnContext, state, cancellationToken);
                    await LanguageDialogSet.StartOrContinueDialogAsync(turnContext, cancellationToken);
                }
                else if (ordering || (intention == StaticTexts.StartOrderIntention && !viewing))
                {
                    await SalesDialogSet.StartOrContinueDialogAsync(turnContext, cancellationToken);
                }
                else if (viewing || intention == StaticTexts.ViewOrdersIntention)
                {
                    await ViewOrdersDialogSet.StartOrContinueDialogAsync(turnContext, cancellationToken);
                }
                else if (intention == StaticTexts.GreetingIntention)
                {
                    await Chatter.SendMessageAsync(turnContext, StaticTexts.GreetingText);
                }
                else if (intention == StaticTexts.JokeIntention)
                {
                    await Chatter.SendMessageAsync(turnContext, StaticTexts.JokeText);
                }
                else
                {
                    await Chatter.SendMessageAsync(turnContext, StaticTexts.NoneText);
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded.Any())
                {
                    if (turnContext.Activity.MembersAdded[0].Name == "User")
                    {
                        if (!state.BotWelcomedUser)
                        {
                            await LanguageDialogSet.StartOrContinueDialogAsync(turnContext, cancellationToken);
                            state.BotWelcomedUser = true;
                            await Accessors.CustomerDataState.SetAsync(turnContext, state, cancellationToken);
                        }
                    }
                }
            }
            await Accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private async Task EndAllDialogsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await SalesDialogSet.EndDialogAsync(turnContext, cancellationToken);
            await LanguageDialogSet.EndDialogAsync(turnContext, cancellationToken);
            await ViewOrdersDialogSet.EndDialogAsync(turnContext, cancellationToken);
        }
    }
}
