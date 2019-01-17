namespace EDMChatBot.NAVClient.Dtos
{
    public class PostSalesOrderLineDto
    {
        public string itemId { get; set; }
        public string lineType { get; set; }
        public int quantity { get; set; }
    }
}