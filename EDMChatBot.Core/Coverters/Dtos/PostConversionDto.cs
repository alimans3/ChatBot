namespace EDMChatBot.Core.Coverters
{
    public class PostConversionDto
    {
        public string input { get; set; }
        public string file { get; set; }
        public string outputformat { get; set; }
        public string filename { get; set; }
        public ConvertOptionsDto converteroptions { get; set; }
    }
}