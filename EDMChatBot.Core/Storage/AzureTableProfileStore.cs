using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace EDMChatBot.Core.Storage
{
    public class AzureTableProfileStore : IProfileStore
    {
        private ICloudTable Table;
        
        public AzureTableProfileStore(ICloudTable table)
        {
            Table = table;
        }
        
        public async Task AddOrUpdateProfile(string key, Profile profile)
        {
            var entity = new AzureTableEntity
            {
                PartitionKey = key,
                RowKey = key,
                Id = profile.Id,
                Enrollments = profile.Enrollments
            };
            var createOperation = TableOperation.InsertOrReplace(entity);
            await Table.ExecuteAsync(createOperation);
        }

        public async Task<Profile> GetProfile(string key)
        {
            var retrieveOperation = TableOperation.Retrieve<AzureTableEntity>(key, key);
            var result = await Table.ExecuteAsync(retrieveOperation);
            if (result.HttpStatusCode == 404)
            {
                throw new InvalidDataException();
            }
            var entity = (AzureTableEntity) result.Result;
            return new Profile
            {
                Id = entity.Id,
                Enrollments = entity.Enrollments
            };
        }

        public async Task<bool> ProfileExists(string key)
        {
            var retrieveOperation = TableOperation.Retrieve<AzureTableEntity>(key, key);
            var result = await Table.ExecuteAsync(retrieveOperation);
            if (result.HttpStatusCode == 404)
            {
                return false;
            }

            return true;
        }
    }
}