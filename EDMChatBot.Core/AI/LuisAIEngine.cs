using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.Settings;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace EDMChatBot.Core.AI
{
    public class LuisAIEngine : INaturalLanguageEngine
    {
        private string EndpointUri;

        private HttpClient Client;
        
        public LuisAIEngine(LuisSettings settings)
        {
            Client = new HttpClient();
            EndpointUri = settings.EndpointUri;
        }

        public async Task<string> RecognizeIntention(string text, CancellationToken token)
        {
            try
            {
                var response = await Client.GetAsync(EndpointUri + text, token);
                var content = await response.Content.ReadAsStringAsync();
                var luisResult = JsonConvert.DeserializeObject<LuisDto>(content);
                if (luisResult.TopScoringIntent.Score < 0.3)
                {
                    return "None";
                }

                return luisResult.TopScoringIntent.Intent;
            }
            catch (Exception)
            {
                return "None";
            }
        }
    }
}