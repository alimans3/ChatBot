using System.Collections.Generic;

namespace EDMChatBot.Core.AI
{
    public class TextTranslatorResultDto
    {
        public LanguageDto DetectedLanguage { get; set; }
        public List<TranslationDto> Translations { get; set; }
    }
}