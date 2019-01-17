using System;

namespace EDMChatBot.Core.Coverters
{
    public class GetProcessDto
    {
        public string url { get; set; }
        public string Id { get; set; }
        public string Host { get; set; }
        public int Maxtime { get; set; }
        public int Minutes { get; set; }
    }
}