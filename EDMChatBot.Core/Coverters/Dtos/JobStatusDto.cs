using System.Collections.Generic;

namespace EDMChatBot.Core.Coverters
{
    public class JobStatusDto
    {
        public string Id { get; set; }
        public StatusDto Status { get; set; }
        public List<OutputDto> Output { get; set; }
    }
}