using Microsoft.WindowsAzure.Storage.Table;

namespace EDMChatBot.Core.Storage
{
    public class AzureTableEntity : TableEntity
    {
        public string Id { get; set; }
        public int Enrollments { get; set; }
    }
}