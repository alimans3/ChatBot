using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace EDMChatBot.Core.Dialogs
{
    public interface IDialog
    {
        WaterfallStep[] GetWaterfallSteps();

        string GetName();

        List<Dialog> GetPrompts();
    }
}