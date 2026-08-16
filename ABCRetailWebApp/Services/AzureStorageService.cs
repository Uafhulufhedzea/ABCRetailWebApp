using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;

namespace ABCRetailWebApp.Services
{
    public class AzureStorageService
    {
        private readonly string _connectionString;

        public AzureStorageService(IConfiguration configuration)
        {
          
            _connectionString = configuration.GetConnectionString("AzureStorage")
                ?? throw new ArgumentNullException("AzureStorage connection string is missing from appsettings.json!");
        }

        public BlobServiceClient GetBlobClient() => new(_connectionString);
        public TableServiceClient GetTableClient() => new(_connectionString);
        public QueueServiceClient GetQueueClient() => new(_connectionString);
        public ShareServiceClient GetFileClient() => new(_connectionString);
    }
}
