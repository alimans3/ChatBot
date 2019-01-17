namespace EDMChatBot.Core.Coverters
{
    public class OptionsDto
    {
        public int frequency { get; set; } 
        public string channels { get; set; }
        public int audio_bitdepth { get; set; }
        public bool normalize { get; set; }
    }
}