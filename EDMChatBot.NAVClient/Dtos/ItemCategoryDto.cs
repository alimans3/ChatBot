using System;

namespace EDMChatBot.NAVClient.Dtos
{
    public class ItemCategoryDto
    {
        public string Id { get; set; }
        
        public string Code { get; set; }
        
        public string DisplayName { get; set; }
        
        public DateTime LastModifiedDateTime { get; set; }
    }
}