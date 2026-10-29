using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;

        public BlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadAsync(
            Stream content, string fileName, string contentType)
        {
            // Created lazily so the app and tests start without Azure config.
            string connectionString =
                _configuration["AzureBlob:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Blob connection string is missing.");

            string containerName =
                _configuration["AzureBlob:ContainerName"]
                ?? "raceday-images";

            var container = new BlobContainerClient(
                connectionString, containerName);

            await container.CreateIfNotExistsAsync(
                PublicAccessType.Blob);

            // Unique name stops one upload overwriting another.
            string extension =
                Path.GetExtension(fileName).ToLowerInvariant();
            string blobName = $"{Guid.NewGuid():N}{extension}";

            var blob = container.GetBlobClient(blobName);

            await blob.UploadAsync(content, new BlobHttpHeaders
            {
                ContentType = contentType
            });

            return blob.Uri.ToString();
        }
    }

    public interface IBlobStorageService
    {
        Task<string?> UploadAsync(Stream stream, string fileName, string contentType);
    }
}