using System;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.NAVClient.Dtos;

namespace EDMChatBot.Core.AI
{
    public interface IFaceRecognizer
    {
        Task<bool> AuthorizeCustomerAsync(CustomerDto customerDto,string imageUrl, CancellationToken cancellationToken);

        Task AddCustomerWithPictureAsync(CustomerDto customerDto,string imageUrl, CancellationToken cancellationToken);

        Task<bool> CheckIfCustomerHasPersonAsync(CustomerDto customerDto, CancellationToken cancellationToken);
    }
}