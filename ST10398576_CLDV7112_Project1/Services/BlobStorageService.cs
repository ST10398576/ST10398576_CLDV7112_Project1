using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace ST10398576_CLDV7112_Project1.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<List<string>> ListBlobUrlsAsync();

        Task<(Stream stream, string contentType, long length)> DownloadBlobAsync(string blobName);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly string _accountName;
        private readonly string _accountKey;

        public BlobStorageService(IConfiguration config)
        {
            string connectionString = config.GetValue<string>("AzureStorage:ConnectionString")!;
            string containerName = "productimages";

            _containerClient = new BlobContainerClient(connectionString, containerName);

            // No PublicAccessType specified -> container stays private.
            _containerClient.CreateIfNotExists();

            (_accountName, _accountKey) = ParseAccountCredentials(connectionString);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            string blobName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });
            }

            return GenerateReadOnlySasUrl(blobClient);
        }

        public async Task<List<string>> ListBlobUrlsAsync()
        {
            var urls = new List<string>();
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                urls.Add(GenerateReadOnlySasUrl(blobClient));
            }
            return urls;
        }

        public async Task<(Stream stream, string contentType, long length)> DownloadBlobAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadAsync();
            var download = response.Value;

            var ms = new MemoryStream();
            await download.Content.CopyToAsync(ms);
            ms.Position = 0;

            var contentType = download.ContentType ?? "application/octet-stream";
            var length = download.ContentLength;
            return (ms, contentType, length);
        }

        private string GenerateReadOnlySasUrl(BlobClient blobClient)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(7)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var credential = new StorageSharedKeyCredential(_accountName, _accountKey);
            var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

            return $"{blobClient.Uri}?{sasToken}";
        }

        private static (string accountName, string accountKey) ParseAccountCredentials(string connectionString)
        {
            var parts = connectionString
                .Split(';')
                .Where(s => !string.IsNullOrWhiteSpace(s) && s.Contains('='))
                .Select(s => s.Split('=', 2))
                .ToDictionary(kv => kv[0], kv => kv[1]);

            return (parts["AccountName"], parts["AccountKey"]);
        }
    }
}
