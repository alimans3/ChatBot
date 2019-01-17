using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EDMChatBot.NAVClient.Dtos;

namespace EDMChatBot.Core.AI
{
    public interface IVoiceVerifier
    {
        Task<bool> AuthorizeCustomerAsync(CustomerDto customerDto,string audioUrl);

        Task AddCustomerProfileAsync(CustomerDto customerDto);

        Task AddCustomerEnrollmentAsync(CustomerDto customerDto,string audioUrl);
        
        Task<bool> CheckIfCustomerHasProfileAsync(CustomerDto customerDto);

        Task<string> GetEnrollmentPhraseAsync();

        Task<int> GetNumberOfEnrollments(CustomerDto customerDto);
    }
}