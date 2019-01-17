using System.Collections.Generic;

namespace EDMChatBot.Core.Resources
{
    public class Column
    {
        public string type { get; set; }
        
        public string width { get; set; }
        
        public List<CardBlock> items { get; set; }
    }
}