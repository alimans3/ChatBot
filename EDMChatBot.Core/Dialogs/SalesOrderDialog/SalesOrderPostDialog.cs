using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.AI;
using EDMChatBot.NAVClient;
using EDMChatBot.NAVClient.Dtos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace EDMChatBot.Core.Dialogs
{
    public class SalesOrderPostDialog : ISalesOrderPostDialog
    {
        public static readonly string ConfirmSalesOrderPrompt = nameof(ConfirmSalesOrderPrompt);
        private WaterfallStep[] WaterfallSteps;
        public static string Name = nameof(SalesOrderPostDialog);
        private SalesDialogBotAccessors Accessors;
        private INavClient Client;
        private List<Dialog> Prompts;
        private IChatter Chatter;

        public SalesOrderPostDialog(SalesDialogBotAccessors accessors, INavClient client, IChatter chatter)
        {
            Chatter = chatter;
            Client = client;
            Accessors = accessors;
            WaterfallSteps = new WaterfallStep[]
            {
                ConfirmSalesOrderStepAsync,
                PostSalesOrderAndViewStepAsync
            };
            Prompts = new List<Dialog>
            {
                new ConfirmPrompt(ConfirmSalesOrderPrompt)
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

        public async Task<DialogTurnResult> ConfirmSalesOrderStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            var attachment = StaticTexts.CreateSalesOrderAttachment(state.TempSalesOrderLines);
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.TempOrderViewedText);
            return await stepContext.PromptAsync(ConfirmSalesOrderPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.ConfirmSalesOrderText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.ConfirmSalesOrderText)
            }, cancellationToken);
        }

        public async Task<DialogTurnResult> PostSalesOrderAndViewStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var result = (bool) stepContext.Result;
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            var lines = state.TempSalesOrderLines;
            if (!result)
            {
                await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.OrderCanceledText);
            }
            else
            {
                var postSalesOrderDto = new PostSalesOrderDto
                {
                    customerNumber = state.Customer.Number,
                    currencyCode = "USD"
                };
                var getOrderDto = await Client.PostSalesOrder(postSalesOrderDto);
                var linesDtos = new List<GetSalesOrderLineDto>();
                foreach (var line in lines)
                {
                    var postSalesOrderLineDto = new PostSalesOrderLineDto
                    {
                        itemId = line.Item.Id,
                        quantity = line.Quantity,
                        lineType = "Item"
                    };
                    var returnedDto = await Client.PostSalesOrderLine(getOrderDto.Id, postSalesOrderLineDto);
                    linesDtos.Add(returnedDto);
                }

                var finalOrder = await Client.GetSalesOrder(getOrderDto.Id);
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Attachment(
                        StaticTexts.CreatePostedSalesOrderAttachment(finalOrder, linesDtos, state.Customer)),
                    cancellationToken: cancellationToken);
                await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.OrderPostedText);
            }
            await Accessors.CustomerDataState.SetAsync(stepContext.Context, state, cancellationToken);
            await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.AnythingElseText);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}