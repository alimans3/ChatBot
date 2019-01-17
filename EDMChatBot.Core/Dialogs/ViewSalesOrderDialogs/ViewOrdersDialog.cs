using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.NAVClient;
using EDMChatBot.NAVClient.Dtos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace EDMChatBot.Core.Dialogs.ViewSalesOrderDialogs
{
    public class ViewOrdersDialog : IViewOrdersDialog
    {
        public static readonly string OrdersPrompt = nameof(OrdersPrompt);
        public static readonly string ActionPrompt = nameof(ActionPrompt);
        public static readonly string OtherActionPrompt = nameof(OtherActionPrompt);
        private WaterfallStep[] WaterfallSteps;
        public static string Name = nameof(ViewOrdersDialog);
        private SalesDialogBotAccessors Accessors;
        private INavClient Client;
        private List<Dialog> Prompts;
        private IChatter Chatter;
        

        
        public ViewOrdersDialog(SalesDialogBotAccessors accessors, INavClient client,IChatter chatter)
        {
            Chatter = chatter;
            Client = client;
            Accessors = accessors;
            WaterfallSteps = new WaterfallStep[]
            {
                ChooseOrderStepAsync,
                ChooseActionStepAsync,
                ToActionStepAsync,
                AnotherActionStepAsync
            };
            Prompts = new List<Dialog>
            {
                new TextPrompt(OrdersPrompt, OrderNumberValidatorAsync),
                new ChoicePrompt(ActionPrompt),
                new ConfirmPrompt(OtherActionPrompt)
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

        public async Task<DialogTurnResult> ChooseOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context, null, cancellationToken);
            if (state.SalesOrders == null)
            {
                var orders = await Client.GetSalesOrdersOfCustomer(state.Customer.Number);
                state.SalesOrders = ConvertDictionary(orders);
                var attachments = new List<Attachment>();
                foreach (var order in orders)
                {
                    attachments.Add(StaticTexts.CreatePostedSalesOrderAttachment(order.Key, order.Value, state.Customer));
                }

                await Accessors.CustomerDataState.SetAsync(stepContext.Context, state, cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Carousel(attachments), cancellationToken);
                await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.ViewedOrdersText);
            }
            return await stepContext.PromptAsync(OrdersPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context, StaticTexts.EnterOrderNumberText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context, StaticTexts.EnterOrderNumberAgainText)
            },cancellationToken: cancellationToken);
        }

        public async Task<DialogTurnResult> ChooseActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context, null, cancellationToken);
            var choosenOrder = await Client.GetSalesOrderByNumber(state.ChoosenOrderNumber);
            var attachment = StaticTexts.CreatePostedSalesOrderAttachment(choosenOrder,
                state.SalesOrders[state.ChoosenOrderNumber], state.Customer);
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.YourSelectedOrderText);
            return await stepContext.PromptAsync(ActionPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context, StaticTexts.OrderActionText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context, StaticTexts.OrderActionText),
                Choices = ChoiceFactory.ToChoices(new List<string>{"Delete","Done"})
            }, cancellationToken);
        }

        public async Task<DialogTurnResult> ToActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context, null, cancellationToken);
            var result = (FoundChoice) stepContext.Result;
            if (result.Value == "Delete")
            {
                await Client.DeleteSalesOrder(state.ChoosenOrderNumber);
                state.SalesOrders.Remove(state.ChoosenOrderNumber);
                await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.DeletedOrderText);
            }
            
            return await stepContext.PromptAsync(OtherActionPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.OtherActionText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.OtherActionText)
            }, cancellationToken);
        }
        
        public async Task<DialogTurnResult> AnotherActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool) stepContext.Result;
            if (result)
            {
                return await stepContext.BeginDialogAsync(Name, null, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        public async Task<bool> OrderNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(promptContext.Context, null, cancellationToken);
            if (!promptContext.Recognized.Succeeded)
            {
                return false;
            }
            var orderExists = state.SalesOrders.TryGetValue(promptContext.Recognized.Value, out List<GetSalesOrderLineDto> value);
            if (orderExists)
            {
                state.ChoosenOrderNumber = promptContext.Recognized.Value;
                await Accessors.CustomerDataState.SetAsync(promptContext.Context, state, cancellationToken);
                return true;
            }
            return false;
        }
        
        private IDictionary<string,List<GetSalesOrderLineDto>> ConvertDictionary(
            IDictionary<GetSalesOrderDto, List<GetSalesOrderLineDto>> dict)
        {
            var dictReturn = new Dictionary<string,List<GetSalesOrderLineDto>>();
            foreach (var order in dict)
            {
                dictReturn.Add(order.Key.Number,order.Value);
            }

            return dictReturn;
        }
    }
}