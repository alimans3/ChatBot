using System.Collections.Generic;

namespace EDMChatBot.Core.Coverters
{
    public class PostJobDto
    {
        public List<JobInputDto> input { get; set; }
        public List<JobConversionDto> conversion { get; set; }
    }
}