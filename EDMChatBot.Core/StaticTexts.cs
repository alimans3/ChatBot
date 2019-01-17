using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using EDMChatBot.Core.Resources;
using EDMChatBot.NAVClient.Dtos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Recognizers.Text.Matcher;
using Newtonsoft.Json;

namespace EDMChatBot.Core
{
    public static class StaticTexts
    {
        public static string ErrorText = "Sorry, something went wrong!";
        public static string WelcomeOrderText = "Welcome to our Order System!";
        public static string CustomerIdText = "What's you name? (in English)";
        public static string CustomerNotFoundText = "Sorry, this name was not found! What's your name?";
        public static string ChooseProductText = "Choose one of the following products:";
        public static string EnterQuantityText = "Enter the wanted quantity:";
        public static string AnotherProductText = "Do you want to order another product?";
        public static string ChooseProductAgainText = "Sorry, This product is not available! " + ChooseProductText;
        public static string EnterQuantityAgainText = "Sorry we don't have this quantity! " + EnterQuantityText;
        public static string StartOrderIntention = "StartOrder";
        public static string JokeIntention = "Joke";
        public static string GreetingIntention = "Greeting";
        public static string ChangeLanguageIntention = "SetLanguage";
        public static string GreetingText = "Hi, how can I help you?";
        public static string JokeText = "One night, I paid $20 to see Prince. But I partied like it was $19.99.";
        public static string ConfirmProfileText = "Do you confirm that this is your profile?";
        public static string ChooseItemCategoryText = "Choose one of the following categories:";
        public static string ChooseItemCategoryAgainText = "Sorry, This category is not available!" + ChooseItemCategoryText;
        public static string NoItemCategoriesExistText = "Sorry, there is no item categories to choose from.";
        public static string NoItemsExistText = "Sorry, there is no items in the selected category to choose from.";
        public static string ItemViewedText = "This is the description of your chosen item";
        public static string TempOrderViewedText = "This is your order!";
        public static string ConfirmSalesOrderText = "Do you want to post this order?";
        public static string OrderPostedText = "Thank you for your order. \n We will contact you shortly!";
        public static string OrderCanceledText = "You canceled your order.";
        public static string StartOverIntention = "StartOver";
        public static string HelpIntention = "Help";
        public static string HelpText = "Type cancel to start over!";
        public static string NoneText = "My bot brain can't understand what you just said!";
        public static string CancelText = "You just canceled the process";
        public static string AnythingElseText = "What else can I do for you?";
        public static string PersonCreatedText = "We added your face to our database.";
        public static string VerifyByPictureText = "Please take a picture of yourself and send it.";
        public static string PictureMoreOnePersonText = "The picture you sent contains none or more than one person.";
        public static string PictureNotValidText = "The picture you sent is not valid.";
        public static string AuthorizedText = "You've been authorized!";
        public static string PictureNotAuthenticText = "The picture you sent is not authentic";
        public static string ChooseLanguageText = "Please choose one of the following languages: \n" +
                                                  "الرجاء اختيار واحدة من اللغات التالية\n" +
                                                  "Por favor elige uno de los siguientes idiomas \n";
        public static string ChooseLanguageAgainText = "The language you choose is not valid! \n " + ChooseLanguageText;
        public static string WelcomeText = "I am a bot and I can help you: \n " +
                                           "1- Order products \n" +
                                           "2- View existing orders \n" +
                                           "3- Understand Voice notes (English only) \n" +
                                           "4- Authenticate Customers using speaker recognition \n" +
                                           "5- Speak and Understand six Different Languages \n" +
                                           "6- Edit and Delete existing orders (soon) \n" +
                                           "7- Add new Customer (soon) \n";
        public static string ViewOrdersIntention = "ViewOrders";
        public static string ViewedOrdersText = "These are your orders.";
        public static string VoicesAddedText = "Your voices has been added to our database";
        public static string UploadVoicesIntroText = "You'll have to upload 3 voice notes to create a profile";
        public static string EnterOrderNumberText = "Enter the order number you wish to view or delete:";
        public static string EnterOrderNumberAgainText = "The number you entered is invalid! " + EnterOrderNumberText;
        public static string YourSelectedOrderText = "This is you selected order";
        public static string OrderActionText = "What would you like to do?";
        public static string DeletedOrderText = "Your order has been deleted";
        public static string OtherActionText = "Do you want to edit another order?";
        public static string UploadVerificationVoiceText(string phrase)
        {
            return 
                $"Please upload a voice note saying:\n \"{phrase}\" \n to identify your identity";
        }
        
        public static string VoiceNotAuthenticText(string phrase)
        {
            return 
                $"The voice you uploaded is not authentic\n" + UploadVerificationVoiceText(phrase);
        }
        
        public static string UploadVoiceText(string phrase, int count)
        {
            return $"{count}- Please Upload a voice note saying:\n \"{phrase}\"";
        } 

        public static CustomerData SetCustomerData(bool isOrdering, string choosenLanguage, bool welcomed,
            CustomerDto customerDto, string currentIntention, bool isViewing)
        {
            return new CustomerData
            {
                Customer = customerDto,
                isOrdering = isOrdering,
                ItemCategories = null,
                Items = null,
                TempItem = null,
                TempItemCategory = null,
                TempQuantity = 0,
                TempSalesOrderLines = null,
                ChoosenLanguage = choosenLanguage,
                BotWelcomedUser = welcomed,
                UnAuthorizedCustomer = null,
                CurrentIntention = currentIntention,
                isViewing = isViewing,
                SalesOrders = null,
                ChoosenOrderNumber = null
            };
        }

        public static string ConfirmQuantityText(int quantity, string productName)
        {
            return $"Do you confirm that you want {quantity} of {productName}";
        }
        
        public static Attachment CreateSalesOrderAttachment(List<SalesOrderLine> lines)
        {
            var titleBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Center",
                isSubtle = false,
                text = "Sales Order Items",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var itemBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = "Item",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = null
            };
            
            var quantityBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "center",
                isSubtle = false,
                text = "Quantity",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = null
            };
            
            var pricesBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "right",
                isSubtle = false,
                text = "Total Price",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = null
            };
            
            var itemColumn = new Column
            {
                type = "Column",
                width = "1.2",
                items= new List<CardBlock>{itemBlock}
            };
                
            var quantityColumn = new Column
            {
                type = "Column",
                width = "1.2",
                items = new List<CardBlock>{quantityBlock}
            };
    
            var priceColumn = new Column
            {
                type = "Column",
                width = "1.2",
                items = new List<CardBlock>{pricesBlock}
            };
            
            var headerSet = new ColumnSet
            {
                separator = false,
                spacing = "medium",
                type = "ColumnSet",
                columns = new List<Column>{itemColumn,quantityColumn,priceColumn}
            };
            
            var sets = new List<ColumnSet>();
            sets.Add(headerSet);
            
            foreach (var line in lines)
            {
                var lineNameBlock = new TextBlock
                {
                    type = "TextBlock",
                    horizontalAlignment = "left",
                    isSubtle = false,
                    text = line.Item.DisplayName,
                    color = null,
                    separator = true,
                    size = null,
                    spacing = null,
                    weight = null
                };
                
                var lineQuantityBlock = new TextBlock
                {
                    type = "TextBlock",
                    horizontalAlignment = "center",
                    isSubtle = false,
                    text = line.Quantity.ToString(),
                    color = null,
                    separator = true,
                    size = null,
                    spacing = null,
                    weight = null
                };
                
                var linePriceBlock = new TextBlock
                {
                    type = "TextBlock",
                    horizontalAlignment = "right",
                    isSubtle = false,
                    text = line.Quantity * line.UnitPrice + "$",
                    color = null,
                    separator = true,
                    size = null,
                    spacing = null,
                    weight = null
                };
                
                var lineNameColumn = new Column
                {
                    type = "Column",
                    width = "1.2",
                    items= new List<CardBlock>{lineNameBlock}
                };
                
                var lineQuantityColumn = new Column
                {
                    type = "Column",
                    width = "1.2",
                    items = new List<CardBlock>{lineQuantityBlock}
                };
                
                var linePriceColumn = new Column
                {
                    type = "Column",
                    width = "1.2",
                    items = new List<CardBlock>{linePriceBlock}
                };
                
                var set = new ColumnSet
                {
                    separator = false,
                    spacing = "medium",
                    type = "ColumnSet",
                    columns = new List<Column>{lineNameColumn,lineQuantityColumn,linePriceColumn}
                };
                
                sets.Add(set);
            }

            var blocks = new List<CardBlock>();
            blocks.Add(titleBlock);
            blocks.AddRange(sets);
            SmallerTextBlocks(blocks);
            var adaptiveCard = new AdaptiveCard
            {
                type = "AdaptiveCard",
                schema = null,
                version = "1.0",
                speak = "Your Order have been confirmed",
                body = blocks
            };

            var adaptive = JsonConvert.SerializeObject(adaptiveCard);
            var card = JsonConvert.DeserializeObject(adaptive);
             
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card
            };
            return adaptiveCardAttachment;
        }

        public static Attachment CreatePostedSalesOrderAttachment(GetSalesOrderDto getSalesOrderDto,
            List<GetSalesOrderLineDto> salesOrderLinesDto, CustomerDto customerDto)
        {
            string seperate = ":      ";
            
            
            var numberBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Center",
                isSubtle = false,
                text = "OrderNumber" + seperate + getSalesOrderDto.Number,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var generalTitleBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Center",
                isSubtle = false,
                text = "General",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var customerNameBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(customerDto.DisplayName) + seperate + customerDto.DisplayName,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };

            var orderDateBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(getSalesOrderDto.OrderDate) + seperate +
                       getSalesOrderDto.OrderDate.Date.ToString("M/d/yy"),
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var generalColumnOne = new Column
            {
                items = new List<CardBlock>{customerNameBlock,orderDateBlock},
                type = "Column",
                width = "1.5"
            };
            
            var requestedDelivaryBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = "DeliveryDate" + seperate + getSalesOrderDto.RequestedDeliveryDate.Date.ToString("M/d/yy"),
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var statusBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = "Status" + seperate + getSalesOrderDto.Status,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var generalColumnTwo = new Column
            {
                items = new List<CardBlock>{requestedDelivaryBlock,statusBlock},
                type = "Column",
                width = "1.5"
            };

            var generalSet = new ColumnSet
            {
                columns = new List<Column>{generalColumnOne,generalColumnTwo},
                separator = true,
                spacing = "medium",
                type = "ColumnSet"
            };
            
            
            var LinesTitleBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Center",
                isSubtle = false,
                text = "Lines",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var descriptionBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = "Description",
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var quantityBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = "No.",
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var unitOfMeasureBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = "Unit",
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var unitPriceExcludingVatBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = "Unit Price",
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var lineAmountExcludingVatBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = "Line Amount",
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = "bolder"
            };

            
            var descriptionColumn = new Column
            {
                items = new List<CardBlock>{descriptionBlock},
                type = "Column",
                width = "4"
            };
            
            var quantityColumn = new Column
            {
                items = new List<CardBlock>{quantityBlock},
                type = "Column",
                width = "1"
            };
            
            var unitOfMeasureColumn = new Column
            {
                items = new List<CardBlock>{unitOfMeasureBlock},
                type = "Column",
                width = "1.5"
            };
            
            var unitPriceExcludingVatColumn = new Column
            {
                items = new List<CardBlock>{unitPriceExcludingVatBlock},
                type = "Column",
                width = "3"
            };
            
            var lineAmountExcludingVatColumn = new Column
            {
                items = new List<CardBlock>{lineAmountExcludingVatBlock},
                type = "Column",
                width = "3"
            };

            foreach (var line in salesOrderLinesDto)
            {

                var descriptionLineBlock = new TextBlock
                {
                    type = "TextBlock",
                    horizontalAlignment = "left",
                    isSubtle = false,
                    text = line.Description,
                    color = null,
                    separator = false,
                    size = null,
                    spacing = null,
                    weight = null
                };

                var quantityLineBlock = new TextBlock
                {
                    type = "TextBlock",
                    horizontalAlignment = "left",
                    isSubtle = false,
                    text = line.Quantity.ToString(),
                    color = null,
                    separator = false,
                    size = null,
                    spacing = null,
                    weight = null
                };

                var unitOfMeasureLineBlock = new TextBlock
                {
                    type = "TextBlock",
                    horizontalAlignment = "left",
                    isSubtle = false,
                    text = line.UnitOfMeasure.Code,
                    color = null,
                    separator = false,
                    size = null,
                    spacing = null,
                    weight = null
                };

                var unitPriceExcludingVatLineBlock = new TextBlock
                {
                    type = "TextBlock",
                    horizontalAlignment = "left",
                    isSubtle = false,
                    text = line.UnitPrice.ToString() + getSalesOrderDto.CurrencyCode,
                    color = null,
                    separator = false,
                    size = null,
                    spacing = null,
                    weight = null
                };

                var lineAmountExcludingVatLineBlock = new TextBlock
                {
                    type = "TextBlock",
                    horizontalAlignment = "left",
                    isSubtle = false,
                    text = line.AmountExcludingTax.ToString() + getSalesOrderDto.CurrencyCode,
                    color = null,
                    separator = false,
                    size = null,
                    spacing = null,
                    weight = null
                };
                
                descriptionColumn.items.Add(descriptionLineBlock);
                quantityColumn.items.Add(quantityLineBlock);
                unitOfMeasureColumn.items.Add(unitOfMeasureLineBlock);
                unitPriceExcludingVatColumn.items.Add(unitPriceExcludingVatLineBlock);
                lineAmountExcludingVatColumn.items.Add(lineAmountExcludingVatLineBlock);
            }
            var TotalExcludingVatBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = "Total Ex. VAT:",
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var TotalVatBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = "Total VAT:",
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var TotalIncludingVatBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = "Total Inc. VAT:",
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            unitPriceExcludingVatColumn.items.Add(TotalExcludingVatBlock);
            unitPriceExcludingVatColumn.items.Add(TotalVatBlock);
            unitPriceExcludingVatColumn.items.Add(TotalIncludingVatBlock);
            
            var TotalExcludingVatAmountBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = getSalesOrderDto.TotalAmountExcludingTax.ToString() + getSalesOrderDto.CurrencyCode,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var TotalVatAmountBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = getSalesOrderDto.TotalTaxAmount.ToString() + getSalesOrderDto.CurrencyCode,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var TotalIncludingAmountVatBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = getSalesOrderDto.TotalAmountIncludingTax.ToString() + getSalesOrderDto.CurrencyCode,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            lineAmountExcludingVatColumn.items.Add(TotalExcludingVatAmountBlock);
            lineAmountExcludingVatColumn.items.Add(TotalVatAmountBlock);
            lineAmountExcludingVatColumn.items.Add(TotalIncludingAmountVatBlock);
            
            var linesSet = new ColumnSet
            {
                columns = new List<Column>
                {
                    descriptionColumn, quantityColumn, unitOfMeasureColumn, unitPriceExcludingVatColumn,
                    lineAmountExcludingVatColumn
                },
                separator = true,
                spacing = "medium",
                type = "ColumnSet"
            };
          
           
            var blocks = new List<CardBlock>();
            blocks.Add(numberBlock);
            blocks.Add(generalTitleBlock);
            blocks.Add(generalSet);
            blocks.Add(LinesTitleBlock);
            blocks.Add(linesSet);
            SmallerTextBlocks(blocks);
            var block = blocks[0] as TextBlock;
            block.size = "Large";
            blocks[0] = block;
            var adaptiveCard = new AdaptiveCard
            {
                type = "AdaptiveCard",
                schema = null,
                version = "1.0",
                speak = "Your Order have been confirmed",
                body = blocks
            };

            var adaptive = JsonConvert.SerializeObject(adaptiveCard);
            var card = JsonConvert.DeserializeObject(adaptive);
             
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card
            };
            return adaptiveCardAttachment;
        }

        public static Attachment CreateCustomerAttachment(CustomerDto customer)
        {
            string seperate = ":      ";
            var generalTitleBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Center",
                isSubtle = false,
                text = "General",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var noBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(customer.Number) + seperate + customer.Number.ToString(),
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var nameBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.DisplayName) + seperate + customer.DisplayName,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var balanceBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.Balance) + seperate + customer.Balance,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var balanceDueBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.OverdueAmount) + seperate +  customer.OverdueAmount,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var generalColumnOne = new Column
            {
                items = new List<CardBlock>{noBlock,nameBlock,balanceBlock,balanceDueBlock},
                type = "Column",
                width = "1.5"
            };
            
            var blockedBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(customer.Blocked) + seperate + customer.Blocked,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var totalSalesBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.TotalSalesExcludingTax) + seperate + customer.TotalSalesExcludingTax,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var generalColumnTwo = new Column
            {
                items = new List<CardBlock>{blockedBlock,totalSalesBlock},
                type = "Column",
                width = "1.5"
            };

            var generalSet = new ColumnSet
            {
                columns = new List<Column>{generalColumnOne,generalColumnTwo},
                separator = true,
                spacing = "medium",
                type = "ColumnSet"
            };
            
            var contactTitleBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Center",
                isSubtle = false,
                text = "Address & Contact",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var addressBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(customer.Address.Street) + seperate + customer.Address.Street.ToString(),
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var cityBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.Address.City) + seperate + customer.Address.City,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var stateBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.Address.State) + seperate + customer.Address.State,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var postCodeDueBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.Address.PostalCode) + seperate +  customer.Address.PostalCode,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var countryCodeDueBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.Address.CountryLetterCode) + seperate +  customer.Address.CountryLetterCode,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var contactColumnOne = new Column
            {
                items = new List<CardBlock>{addressBlock,cityBlock,stateBlock,postCodeDueBlock,countryCodeDueBlock},
                type = "Column",
                width = "1.5"
            };
            
            var phoneBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(customer.PhoneNumber) + seperate + customer.PhoneNumber,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var emailBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.Email) + seperate + customer.Email,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var homepageBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(customer.Website) + seperate + customer.Website,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var contactColumnTwo = new Column
            {
                items = new List<CardBlock>{phoneBlock,emailBlock,homepageBlock},
                type = "Column",
                width = "1.5"
            };

            var contactSet = new ColumnSet
            {
                columns = new List<Column>{contactColumnOne ,contactColumnTwo},
                separator = true,
                spacing = "medium",
                type = "ColumnSet"
            };
          
           
            var blocks = new List<CardBlock>();
            blocks.Add(generalTitleBlock);
            blocks.Add(generalSet);
            blocks.Add(contactTitleBlock);
            blocks.Add(contactSet);
               
            SmallerTextBlocks(blocks);
            var adaptiveCard = new AdaptiveCard
            {
                type = "AdaptiveCard",
                schema = null,
                version = "1.0",
                speak = "Your Order have been confirmed",
                body = blocks
            };
            var adaptive = JsonConvert.SerializeObject(adaptiveCard);
            var card = JsonConvert.DeserializeObject(adaptive);
             
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card
            };
            return adaptiveCardAttachment;
        }

        public static Attachment CreateItemAttachment(ItemDto item)
        {
            string seperate = ":      ";
            var generalTitleBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Center",
                isSubtle = false,
                text = "General",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var noBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(item.Number) + seperate + item.Number.ToString(),
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var nameBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(item.DisplayName) + seperate + item.DisplayName,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var generalColumnOne = new Column
            {
                items = new List<CardBlock>{noBlock,nameBlock},
                type = "Column",
                width = "1.5"
            };
            
            var blockedBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(item.Blocked) + seperate + item.Blocked,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var categoryBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(item.ItemCategoryCode) + seperate + item.ItemCategoryCode,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var generalColumnTwo = new Column
            {
                items = new List<CardBlock>{blockedBlock,categoryBlock},
                type = "Column",
                width = "1.5"
            };

            var generalSet = new ColumnSet
            {
                columns = new List<Column>{generalColumnOne,generalColumnTwo},
                separator = true,
                spacing = "medium",
                type = "ColumnSet"
            };
            
            var PricingTitleBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Center",
                isSubtle = false,
                text = "Pricing",
                color = null,
                separator = true,
                size = null,
                spacing = null,
                weight = "bolder"
            };
            
            var unitPriceBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(item.UnitPrice) + seperate + item.UnitPrice,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var typeBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "left",
                isSubtle = false,
                text = nameof(item.Type) + seperate + item.Type,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var pricingColumnOne = new Column
            {
                items = new List<CardBlock>{unitPriceBlock,typeBlock},
                type = "Column",
                width = "1.5"
            };
            
            var quantityUnitBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(item.Inventory) + seperate + item.Inventory,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };

            var baseUnitBlock = new TextBlock
            {
                type = "TextBlock",
                horizontalAlignment = "Left",
                isSubtle = false,
                text = nameof(item.BaseUnitOfMeasure) + seperate + item.BaseUnitOfMeasure.Code,
                color = null,
                separator = false,
                size = null,
                spacing = null,
                weight = null
            };
            
            var pricingColumnTwo = new Column
            {
                items = new List<CardBlock>{quantityUnitBlock,baseUnitBlock},
                type = "Column",
                width = "1.5"
            };

            var pricingSet = new ColumnSet
            {
                columns = new List<Column>{pricingColumnOne ,pricingColumnTwo},
                separator = true,
                spacing = "medium",
                type = "ColumnSet"
            };
          
           
            var blocks = new List<CardBlock>();
            blocks.Add(generalTitleBlock);
            blocks.Add(generalSet);
            blocks.Add(PricingTitleBlock);
            blocks.Add(pricingSet);
             
            SmallerTextBlocks(blocks);
            var adaptiveCard = new AdaptiveCard
            {
                type = "AdaptiveCard",
                schema = null,
                version = "1.0",
                speak = "Your Order have been confirmed",
                body = blocks
            };

            var adaptive = JsonConvert.SerializeObject(adaptiveCard);
            var card = JsonConvert.DeserializeObject(adaptive);
             
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card
            };
            return adaptiveCardAttachment;
        }

        public static void SmallerTextBlocks(List<CardBlock> blocks)
        {
            for(int i=0;i<blocks.Count;i++)
            {
                if (blocks[i].type == "ColumnSet")
                {
                    var set = blocks[i] as ColumnSet;
                    foreach (var column in set.columns)
                    {
                        SmallerTextBlocks(column.items);
                    }
                }
                else if (blocks[i].type  == "TextBlock")
                {
                    var textBlock = blocks[i] as TextBlock;
                    textBlock.size = "small";
                    blocks[i] = textBlock;
                }
            }
        }
    }
}