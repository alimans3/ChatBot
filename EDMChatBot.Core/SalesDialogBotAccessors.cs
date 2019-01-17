// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using EDMChatBot.Core;
using EDMChatBot.NAVClient.Dtos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace EDMChatBot.Core
{
    public class SalesDialogBotAccessors
    {
        public SalesDialogBotAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState;
        }

        public IStatePropertyAccessor<DialogState> OrderDialogState { get; set; }
        
        public IStatePropertyAccessor<DialogState> LanguageDialogState { get; set; }
        
        public IStatePropertyAccessor<DialogState> ViewOrdersDialogState { get; set; }
        
        public IStatePropertyAccessor<CustomerData> CustomerDataState { get; set; }
        
        public ConversationState ConversationState { get; }
    }
}
