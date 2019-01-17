using EDMChatBot.NAVClient.Dtos;

namespace EDMChatBot.Core
{
    public class SalesOrderLine
    {
        public ItemDto Item { get; set; }
        
        public int Quantity { get; set; }
        
        public double UnitPrice { get; set; }
    }
}