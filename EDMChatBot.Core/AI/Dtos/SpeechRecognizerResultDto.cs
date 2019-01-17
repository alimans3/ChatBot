using System.Collections.Generic;

namespace EDMChatBot.Core.AI
{
    public class SpeechRecognizerResultDto
    {
        public string RecognitionStatus { get; set; }
        public List<SpeechDetailedDto> NBest { get; set; }
    }
}