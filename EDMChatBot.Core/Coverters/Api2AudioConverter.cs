using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EDMChatBot.Core.Settings;
using Newtonsoft.Json;

namespace EDMChatBot.Core.Coverters
{
    public class Api2AudioConverter : IAudioConverter
    {
        private HttpClient Client;
        
        public Api2AudioConverter(Api2ConverterSettings settings)
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri(settings.EndpointUrl);
            Client.DefaultRequestHeaders.Add("X-Oc-Api-Key",settings.ApiKey);
        }
        
        public async Task<byte[]> ConvertOggToWav(string url)
        {
            var dtoToSend = new PostJobDto
            {
                conversion = new List<JobConversionDto>
                {
                    new JobConversionDto
                    {
                        target = "wav",
                        options = new OptionsDto
                        {
                            frequency = 16000,
                            channels = "mono",
                            audio_bitdepth = 16
                        }
                    }
                },
                input = new List<JobInputDto>
                {
                    new JobInputDto
                    {
                        type = "remote",
                        source = url
                    }
                }
            };

            var response = await Client.PostAsync("jobs",
                new StringContent(JsonConvert.SerializeObject(dtoToSend), Encoding.UTF8, "application/json"));
            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception();
            }
            var status = JsonConvert.DeserializeObject<JobStatusDto>(responseJson);
            var jobId = status.Id;
            
            string returnedUri;
            while (true)
            {
                var statusResponse = await Client.GetAsync($"jobs/{jobId}");
                var statusContent = await statusResponse.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<JobStatusDto>(statusContent);
                if (status.Status.Code == "completed")
                {
                    returnedUri = status.Output[0].Uri;
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200));

            }

            var guid = Guid.NewGuid().ToString();
            var webClient = new WebClient();
            await webClient.DownloadFileTaskAsync(returnedUri, $"{guid}.wav");
            var bytes = File.ReadAllBytes($"{guid}.wav");
            File.Delete($"{guid}.wav");
            return bytes;
        }
    }
}