using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ST10398576_CLDV7112_Project1.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<List<string>> ListBlobUrlsAsync();
    }

        // Download a blob as a stream along with content type and length
        public async Task<(Stream Content, string ContentType, long Length)> DownloadBlobAsync(string blobName)
        {
            if (!_initialized || _containerClient == null)
            {
                throw new InvalidOperationException("Blob storage is not configured or unavailable.");
            }

            var blobClient = _containerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadAsync();
            var headers = response.Value.Details.ContentType ?? "application/octet-stream";
            var length = response.Value.ContentLength;
            return (response.Value.Content, headers, length);
        }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient? _containerClient;
        private readonly bool _initialized;
        private readonly string? _initError;

        public BlobStorageService(IConfiguration config, ILogger<BlobStorageService> logger)
        {
            try
            {
                string? connectionString = config.GetValue<string>("AzureStorage:ConnectionString");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _initialized = false;
                    _initError = "Connection string is missing or empty.";
                    logger?.LogWarning("BlobStorageService not initialized: {Reason}", _initError);
                    return;
                }

                string containerName = "productimages";

                _containerClient = new BlobContainerClient(connectionString, containerName);

                _containerClient.CreateIfNotExists(PublicAccessType.Blob);
                _initialized = true;
                logger?.LogInformation("BlobStorageService initialized for container '{Container}'", containerName);
            }
            catch (Exception ex)
            {
                _initialized = false;
                _initError = ex.Message;
                logger?.LogError(ex, "Failed to initialize BlobStorageService: {Message}", ex.Message);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (!_initialized || _containerClient == null)
            {
                var msg = "Blob storage is not configured or unavailable.";
                if (!string.IsNullOrEmpty(_initError)) msg += " Initialization error: " + _initError;
                throw new InvalidOperationException(msg);
            }

            string blobName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });
            }

            return blobClient.Uri.ToString();
        }

        public async Task<List<string>> ListBlobUrlsAsync()
        {
            var urls = new List<string>();
            if (!_initialized || _containerClient == null)
            {
                return urls;
            }

            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                urls.Add(blobClient.Uri.ToString());
            }
            return urls;
        }
    }
}