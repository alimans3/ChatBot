using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.Coverters;
using EDMChatBot.Core.Settings;
using EDMChatBot.Core.Storage;
using EDMChatBot.NAVClient.Dtos;
using Newtonsoft.Json;

namespace EDMChatBot.Core.AI
{
    public class VoiceHttpVerifier : IVoiceVerifier
    {
        private readonly HttpClient Client;
        private readonly IProfileStore Store;
        private readonly IAudioConverter Converter;
        private readonly string Phrase;
        
        public VoiceHttpVerifier(VoiceVerifierSettings settings, IProfileStore store, IAudioConverter converter)
        {
            Converter = converter;
            Client = new HttpClient();
            Phrase = settings.Phrase;
            Client.BaseAddress = new Uri(settings.EndpointUrl);
            Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key",settings.SubscriptionKey);
            Store = store;
        }


        public async Task<bool> AuthorizeCustomerAsync(CustomerDto customerDto, string audioUrl)
        {
            var profile = await Store.GetProfile(customerDto.Id);
            var verificationProfileId = profile.Id;
            var bytes = await Converter.ConvertOggToWav(audioUrl);
            
            var response = await Client.PostAsync($"verify?verificationProfileId={verificationProfileId}",
                new ByteArrayContent(bytes));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var returnedDto = JsonConvert.DeserializeObject<VerificationResultDto>(jsonContent);
            if (returnedDto.Result == "Reject" || (returnedDto.Result == "Accept" & returnedDto.Confidence == "Low"))
            {
                return false;
            }

            await TryAddCustomerEnrollmentAsync(customerDto, bytes);

            return true;

        }

        public async Task AddCustomerProfileAsync(CustomerDto customerDto)
        {
            if (await Store.ProfileExists(customerDto.Id))
            {
                return;
            }
            
            var dto = new PostVerificationProfileDto
            {
                locale = "en-US"
            };
            var result = await Client.PostAsync("verificationProfiles",
                new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json"));
            if (!result.IsSuccessStatusCode)
            {
                throw new VerificationException("Cannot add profile");
            }
            var jsonResponse = await result.Content.ReadAsStringAsync();
            var profile = JsonConvert.DeserializeObject<GetVerificationProfileDto>(jsonResponse);
            await Store.AddOrUpdateProfile(customerDto.Id,new Profile
            {
                Enrollments = 0,
                Id = profile.VerificationProfileId
            });

        }

        public async Task AddCustomerEnrollmentAsync(CustomerDto customerDto, string audioUrl)
        {
            try
            {
                var profile = await Store.GetProfile(customerDto.Id);
                var profileId = profile.Id;
                var bytes = await Converter.ConvertOggToWav(audioUrl);
                var response =
                    await Client.PostAsync($"verificationProfiles/{profileId}/enroll", new ByteArrayContent(bytes));
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new VerificationException("Cannot add enrollment" + response.ReasonPhrase + jsonContent);
                }

                var dtoReturned = JsonConvert.DeserializeObject<EnrollmentDto>(jsonContent);
                await Store.AddOrUpdateProfile(customerDto.Id,new Profile
                {
                    Enrollments = dtoReturned.EnrollmentsCount,
                    Id = profileId
                });
            }
            catch (Exception e)
            {
                throw new VerificationException(e.Message);
            }
        }
        
        private async Task<bool> TryAddCustomerEnrollmentAsync(CustomerDto customerDto, byte[] bytes)
        {
            try
            {
                var profile = await Store.GetProfile(customerDto.Id);
                var profileId = profile.Id;
                var response =
                    await Client.PostAsync($"verificationProfiles/{profileId}/enroll", new ByteArrayContent(bytes));
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new VerificationException("Cannot add enrollment" + response.ReasonPhrase + jsonContent);
                }

                var dtoReturned = JsonConvert.DeserializeObject<EnrollmentDto>(jsonContent);
                await Store.AddOrUpdateProfile(customerDto.Id,new Profile
                {
                    Enrollments = dtoReturned.EnrollmentsCount,
                    Id = profileId
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CheckIfCustomerHasProfileAsync(CustomerDto customerDto)
        {
            return await Store.ProfileExists(customerDto.Id);
        }

        public async Task<int> GetNumberOfEnrollments(CustomerDto customerDto)
        {
            if (!await CheckIfCustomerHasProfileAsync(customerDto))
            {
                return 0;
            }
            var profile = await Store.GetProfile(customerDto.Id);
            var profileId = profile.Id;
            var response = await Client.GetAsync($"verificationProfiles/{profileId}");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new VerificationException(response.ReasonPhrase);
            }

            var enrollmentProfile = JsonConvert.DeserializeObject<EnrollmentDto>(jsonResponse);
            return enrollmentProfile.EnrollmentsCount;
        }

        public Task<string> GetEnrollmentPhraseAsync()
        {
            return Task.FromResult(Phrase);
        }
    }
}