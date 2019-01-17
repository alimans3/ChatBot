using System.Threading.Tasks;

namespace EDMChatBot.Core.Storage
{
    public interface IProfileStore
    {
        Task AddOrUpdateProfile(string key, Profile profile);

        Task<Profile> GetProfile(string key);

        Task<bool> ProfileExists(string key);
    }
}