using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EDMChatBot.Core.Settings;
using Newtonsoft.Json;

namespace EDMChatBot.Core.Coverters
{
    public class CloudConverter : IAudioConverter
    {
        public HttpClient Client;
        
        public CloudConverter(CloudConverterSettings settings)
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",settings.ApiKey);
            Client.BaseAddress = new Uri(settings.EndpointUrl);
        }
        
        public async Task<byte[]> ConvertOggToWav(string url)
        {
            //Start the process
            var postProcessDto = new PostProcessDto
            {
                inputformat = "oga",
                outputformat = "wav"
            };
            var response = await Client.PostAsync("process",
                new StringContent(JsonConvert.SerializeObject(postProcessDto), Encoding.UTF8, "application/json"));
            var getProcessJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception();
            }
            var process = JsonConvert.DeserializeObject<GetProcessDto>(getProcessJson);
            
            //Start conversion
            var postConversionDto = new PostConversionDto
            {
                input = "download",
                file = url,
                outputformat = "wav",
                filename = "input.oga",
                converteroptions = new ConvertOptionsDto
                {
                    audio_bitrate = 16,
                    audio_channels = "mono",
                    audio_frequency = 16000
                }
            };
            var urlString = "https://" + process.Host + "/";
            var token = Client.DefaultRequestHeaders.Authorization.Parameter;
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(urlString);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",token);
                            
            var conversionResponse = await httpClient.PostAsync($"process/{process.Id}",
                new StringContent(JsonConvert.SerializeObject(postConversionDto), Encoding.UTF8, "application/json"));
            var getConversionJson = await conversionResponse.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception();
            }

            var conversion = JsonConvert.DeserializeObject<GetConversionDto>(getConversionJson);

            //Download output File
            string returnedUri;
            int i = 0;
            while (true)
            {
                var outputResponse = await httpClient.GetAsync($"process/{process.Id}");
                var getoutputJson = await outputResponse.Content.ReadAsStringAsync();
                var output = JsonConvert.DeserializeObject<GetConversionDto>(getoutputJson);
                if (output.percent == 100.00)
                {
                    returnedUri = output.output.url;
                    break;
                }

                if (i > 20)
                {
                    throw new Exception();
                }
                i++;
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
            var guid = Guid.NewGuid().ToString();
            var webClient = new WebClient();
            await webClient.DownloadFileTaskAsync("https:" + returnedUri, $"{guid}.wav");
            var bytes = File.ReadAllBytes($"{guid}.wav");
            File.Delete($"{guid}.wav");
            return bytes;
        }
    }
}