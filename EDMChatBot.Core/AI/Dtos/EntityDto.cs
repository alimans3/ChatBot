namespace EDMChatBot.Core.AI
{
    public class EntityDto
    {
        public string Entity { get; set; }
        
        public string Type { get; set; }
        
        public int StartIndex { get; set; }
        
        public int EndIndex { get; set; }
        
        public double Score { get; set; } 
    }
}