using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace EDMChatBot.Core.Storage
{
    public interface ICloudTable
    {
        Task CreateIfNotExistsAsync();
        
        Task<TableResult> ExecuteAsync(TableOperation operation);
    }
}