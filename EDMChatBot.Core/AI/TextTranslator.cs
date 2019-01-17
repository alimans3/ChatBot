using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EDMChatBot.Core.Settings;
using Newtonsoft.Json;

namespace EDMChatBot.Core.AI
{
    public class TextTranslator : ITextTranslator
    {
        private TranslatorApiSettings Settings;
        
        public TextTranslator(TranslatorApiSettings settings)
        {
            Settings = settings;
        }

        public async Task<string> TranslateText(string message, string outputLanguage)
        {
            try
            {
                var body = new List<PostTranslateDto> {new PostTranslateDto {Text = message}};
                var serialized = JsonConvert.SerializeObject(body);
                var extentionUri = Settings.Host + "&to=" + outputLanguage;
                var client = new HttpClient();
                client.BaseAddress = new Uri(Settings.EndpointUrl);
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.SubscriptionKey);
                var response = await client.PostAsync(extentionUri,
                    new StringContent(serialized, Encoding.UTF8, "application/json"));
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var translated = JsonConvert.DeserializeObject<List<TextTranslatorResultDto>>(jsonResponse);
                return translated[0].Translations[0].Text;
            }
            catch (Exception)
            {
                return "";
            }
            
        }

        public Task<List<string>> GetLanguages()
        {
            return Task.FromResult(new List<string>{"en","ar","de","it","fr","es",});
        }
    }
}