using System.Collections.Generic;

namespace EDMChatBot.Core.AI
{
    public class LuisDto
    {
        public string Query { get; set; }
        
        public IntentDto TopScoringIntent { get; set; }
        
        public List<IntentDto> Intents { get; set; }
        
        public List<EntityDto> Entities { get; set; }
    }
}