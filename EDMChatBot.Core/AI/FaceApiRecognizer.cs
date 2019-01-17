using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.Core.Settings;
using EDMChatBot.NAVClient.Dtos;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Newtonsoft.Json;

namespace EDMChatBot.Core.AI
{
    public class FaceApiRecognizer : IFaceRecognizer
    {
        private readonly IFaceClient FaceClient;
        private readonly string PersonGroupId;
        
        public FaceApiRecognizer(IFaceClient faceClient, FaceApiSettings settings)
        {
            FaceClient = faceClient;
            PersonGroupId = settings.PersonGroupId;
        }

        public async Task<bool> AuthorizeCustomerAsync(CustomerDto customerDto, string imageUrl,
            CancellationToken cancellationToken)
        {
            var faces = await FaceClient.Face.DetectWithUrlAsync(imageUrl, cancellationToken: cancellationToken);
            var faceIds = faces.Select(face => face.FaceId.Value).ToArray();
            if (!faces.Any())
            {
                return false;
            }
            var results = await FaceClient.Face.IdentifyAsync(faceIds, largePersonGroupId: PersonGroupId,
                maxNumOfCandidatesReturned: 1, confidenceThreshold: 0.5, cancellationToken: cancellationToken);
            foreach (var result in results)
            {
                if (result.Candidates.Count != 0)
                {
                    var candidateId = result.Candidates[0].PersonId;
                    var person =
                        await FaceClient.LargePersonGroupPerson.GetAsync(PersonGroupId, candidateId, cancellationToken);
                    var dtoToVerify = JsonConvert.DeserializeObject<CustomerDto>(person.UserData);
                    if (dtoToVerify.Id == customerDto.Id)
                    {
                        var faceToConsider = faces.First(face => face.FaceId == result.FaceId);
                        var faceRectangle = faceToConsider.FaceRectangle;
                        var targetFace = new List<int>
                            {faceRectangle.Left, faceRectangle.Top, faceRectangle.Width, faceRectangle.Height};
                        await AddImageToPersonAsync(person.PersonId, imageUrl,targetFace, cancellationToken);
                        return true;
                    }
                }
            }
            return false;
        }

        private async Task AddImageToPersonAsync(Guid personId, string imageUrl, IList<int> targetFace,
            CancellationToken cancellationToken)
        {
            await FaceClient.LargePersonGroupPerson.AddFaceFromUrlAsync(PersonGroupId, personId,
                imageUrl,targetFace: targetFace, cancellationToken: cancellationToken);
            await FaceClient.LargePersonGroup.TrainAsync(PersonGroupId, cancellationToken);
            var taskTraining = FaceClient.LargePersonGroup.GetTrainingStatusAsync(PersonGroupId, cancellationToken);
            
            while (true)
            {
                var value = await FaceClient.LargePersonGroup.GetTrainingStatusAsync(PersonGroupId, cancellationToken);

                if (value.Status == TrainingStatusType.Running || value.Status == TrainingStatusType.Succeeded)
                {
                    break;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(200),cancellationToken);
            }
        }

        public async Task AddCustomerWithPictureAsync(CustomerDto customerDto, string imageUrl,
            CancellationToken cancellationToken)
        {
            await CreateIfNotExistsPersonGroup();
            var detectedFaces =
                await FaceClient.Face.DetectWithUrlAsync(imageUrl, cancellationToken: cancellationToken);
            if (detectedFaces.Count != 1)
            {
                throw new FaceApiException("Zero or more than one face in Picture");
            }
            var person = await FaceClient.LargePersonGroupPerson.CreateAsync(PersonGroupId, customerDto.DisplayName,
                JsonConvert.SerializeObject(customerDto), cancellationToken);
            await AddImageToPersonAsync(person.PersonId, imageUrl,null , cancellationToken);
        }

        public async Task<bool> CheckIfCustomerHasPersonAsync(CustomerDto customerDto,
            CancellationToken cancellationToken)
        {
            await CreateIfNotExistsPersonGroup();
            var persons =
                await FaceClient.LargePersonGroupPerson.ListAsync(PersonGroupId, null, null, cancellationToken);
            foreach (var person in persons)
            {
                var dto = JsonConvert.DeserializeObject<CustomerDto>(person.UserData);
                if (dto.Id == customerDto.Id)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task CreateIfNotExistsPersonGroup()
        {
            try
            {
                await FaceClient.LargePersonGroup.CreateAsync(PersonGroupId, nameof(PersonGroupId));
            }
            catch (Exception)
            {
            }
        }
    }
}