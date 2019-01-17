using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EDMChatBot.Core.Settings;
using Newtonsoft.Json;

namespace EDMChatBot.Core.AI
{
    public class SpeechHttpRecognizer : ISpeechRecognizer
    {
        private SpeechRecognizerSettings Settings;
        
        public SpeechHttpRecognizer(SpeechRecognizerSettings settings)
        {
            Settings = settings;
        }

        public async Task<string> RecognizeSpeech(string audioUrl)
        {
            var webClient = new WebClient();
            var guid = Guid.NewGuid().ToString();
            await webClient.DownloadFileTaskAsync(audioUrl, $"{guid}.ogg");
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Settings.EndpointUrl + "&language=en-US");
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.SubscriptionKey);
            var bytes = File.ReadAllBytes($"{guid}.ogg");
            File.Delete($"{guid}.ogg");
            var response = await httpClient.PostAsync("", new ByteArrayContent(bytes));
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SpeechRecognizerResultDto>(content);
            if (!response.IsSuccessStatusCode || result.RecognitionStatus != "Success" ||
                result.NBest[0].Confidence < 0.5)
            {
                return "";
            }

            return result.NBest[0].Display;

        }
    }
}