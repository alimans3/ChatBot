using System.Collections.Generic;
using EDMChatBot.NAVClient.Dtos;

namespace EDMChatBot.Core
{
    public class CustomerData
    {
        public bool isOrdering { get; set; }
        
        public List<ItemCategoryDto> ItemCategories { get; set; }
        
        public ItemCategoryDto TempItemCategory { get; set; }
        
        public ItemDto TempItem { get; set; }
        
        public int TempQuantity { get; set; }
        
        public List<ItemDto> Items { get; set; }
        
        public List<SalesOrderLine> TempSalesOrderLines { get; set; }
        
        public CustomerDto Customer { get; set; }
        
        public CustomerDto UnAuthorizedCustomer { get; set; }
        
        public string ChoosenLanguage { get; set; }

        public bool BotWelcomedUser { get; set; }
        
        public string CurrentIntention { get; set; }
        
        public bool isViewing { get; set; }
        
        public IDictionary<string,List<GetSalesOrderLineDto>> SalesOrders { get; set; }
        
        public string ChoosenOrderNumber { get; set; }
    }
}