using System.Collections.Generic;

namespace EDMChatBot.Core.Resources
{
    public class AdaptiveCard
    {
        public string schema { get; set; }
        
        public string version { get; set; }
        
        public string type { get; set; }
        
        public string speak { get; set; }
        
        public List<CardBlock> body { get; set; }
    }
}