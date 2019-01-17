using System.Collections.Generic;
using System.Threading.Tasks;

namespace EDMChatBot.Core.AI
{
    public interface ITextTranslator
    {
        Task<string> TranslateText(string message, string outputLanguage);

        Task<List<string>> GetLanguages();
    }
}