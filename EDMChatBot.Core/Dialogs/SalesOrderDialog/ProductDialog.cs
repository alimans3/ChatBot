using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ProductDialog : IProductDialog
    {
        public static readonly string ProductPrompt = nameof(ProductPrompt);
        public static readonly string QuantityPrompt = nameof(QuantityPrompt);
        public static readonly string ItemCategoryPrompt = nameof(ItemCategoryPrompt);
        public static readonly string ConfirmQuantityPrompt = nameof(ConfirmQuantityPrompt);
        private WaterfallStep[] WaterfallSteps;
        public static string Name = nameof(ProductDialog);
        private SalesDialogBotAccessors Accessors;
        private INavClient Client;
        private List<Dialog> Prompts;
        private IChatter Chatter;
        

        
        public ProductDialog(SalesDialogBotAccessors accessors, INavClient client,IChatter chatter)
        {
            Chatter = chatter;
            Client = client;
            Accessors = accessors;
            WaterfallSteps = new WaterfallStep[]
            {
                ChooseItemCategoryStepAsync,
                ChooseItemStepAsync,
                ChooseQuantityStepAsync,
                ConfirmQuantityStepAsync,
                ToAnotherProductStepAsync
            };
            Prompts = new List<Dialog>
            {
                new ChoicePrompt(ItemCategoryPrompt, ItemCategoryChoiceValidatorAsync),
                new ChoicePrompt(ProductPrompt, ProductChoiceValidatorAsync),
                new ConfirmPrompt(ConfirmQuantityPrompt),
                new NumberPrompt<int>(QuantityPrompt, QuantityChoiceValidatorAsync)
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


        public async Task<DialogTurnResult> ChooseItemCategoryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,cancellationToken: cancellationToken);
            var itemCategories= await Client.GetItemCategories();
            
            if (!itemCategories.Any())
            {
                await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.NoItemCategoriesExistText);
                return await stepContext.ReplaceDialogAsync(Name, cancellationToken: cancellationToken);
            }

            state.ItemCategories = itemCategories;
            await Accessors.CustomerDataState.SetAsync(stepContext.Context, state, cancellationToken);
            var converter = new Converter<ItemCategoryDto, string>(input => input.DisplayName);
            return await stepContext.PromptAsync(ItemCategoryPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.ChooseItemCategoryText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.ChooseItemCategoryAgainText),
                Choices = ChoiceFactory.ToChoices(itemCategories.ConvertAll(converter))
            }, cancellationToken);
        }
        
        public async Task<DialogTurnResult> ChooseItemStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var foundChoice = (FoundChoice) stepContext.Result;
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            state.TempItemCategory = state.ItemCategories.First(dto => dto.DisplayName == foundChoice.Value);
            state.Items = await Client.GetItems(state.TempItemCategory);
            if (!state.Items.Any())
            {
                await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.NoItemsExistText);
                return await stepContext.ReplaceDialogAsync(Name, cancellationToken: cancellationToken);
            }

            await Accessors.CustomerDataState.SetAsync(stepContext.Context, state, cancellationToken);
            var converter = new Converter<ItemDto, string>(input => input.DisplayName);
            return await stepContext.PromptAsync(ProductPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.ChooseProductText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.ChooseProductAgainText),
                Choices = ChoiceFactory.ToChoices(state.Items.ConvertAll(converter))
            }, cancellationToken);
        }

        public async Task<DialogTurnResult> ChooseQuantityStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var foundChoice = (FoundChoice) stepContext.Result;
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            if (int.TryParse(foundChoice.Value, out int index))
            {
                state.TempItem = state.Items[index-1];
            }
            else
            {
                state.TempItem = state.Items.First(dto => dto.DisplayName == foundChoice.Value);
            }

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Attachment(StaticTexts.CreateItemAttachment(state.TempItem)), cancellationToken);
            await Chatter.SendMessageAsync(stepContext.Context, StaticTexts.ItemViewedText);
            await Accessors.CustomerDataState.SetAsync(stepContext.Context, state, cancellationToken);
            return await stepContext.PromptAsync(QuantityPrompt, new PromptOptions
            {
                Prompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.EnterQuantityText),
                RetryPrompt = await Chatter.GetActivityAsync(stepContext.Context,StaticTexts.EnterQuantityAgainText)
            }, cancellationToken);
        }
        
        public async Task<DialogTurnResult> ConfirmQuantityStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(stepContext.Context,
                cancellationToken: cancellationToken);
            state.TempQuantity = (int) stepContext.Result;
            var prompt = await Chatter.GetActivityAsync(stepContext.Context,
                StaticTexts.ConfirmQuantityText(state.TempQuantity, state.TempItem.DisplayName));
            await Accessors.CustomerDataState.SetAsync(stepContext.Context, state, cancellationToken);
            return await stepContext.PromptAsync(ConfirmQuantityPrompt, new PromptOptions
            {
                Prompt = prompt,
                RetryPrompt = prompt
            }, cancellationToken);
        }
        
        public async Task<DialogTurnResult> ToAnotherProductStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var result = (bool) stepContext.Result;
            if (result)
            {
                return await stepContext.BeginDialogAsync(AnotherProductDialog.Name,
                    cancellationToken: cancellationToken);
            }
            return await stepContext.BeginDialogAsync(Name,
                cancellationToken: cancellationToken);
        }
        
        public async Task<bool> ProductChoiceValidatorAsync(PromptValidatorContext<FoundChoice> promptContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(promptContext.Context,
                cancellationToken: cancellationToken);
            if (int.TryParse(promptContext.Context.Activity.Text, out int index))
            {
                try
                {
                    return state.Items[index-1] != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return state.Items.Exists(dto => dto.DisplayName == promptContext.Context.Activity.Text);
        }

        public async Task<bool> ItemCategoryChoiceValidatorAsync(PromptValidatorContext<FoundChoice> promptContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(promptContext.Context,
                cancellationToken: cancellationToken);
            return state.ItemCategories.Exists(dto => dto.DisplayName == promptContext.Context.Activity.Text);
        }

        public async Task<bool> QuantityChoiceValidatorAsync(PromptValidatorContext<int> promptContext,
            CancellationToken cancellationToken)
        {
            var state = await Accessors.CustomerDataState.GetAsync(promptContext.Context,
                cancellationToken: cancellationToken);
            return promptContext.Recognized.Value <= state.TempItem.Inventory;
        }
    }
}