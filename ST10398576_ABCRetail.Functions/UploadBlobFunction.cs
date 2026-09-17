using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ST10398576_ABCRetail.Functions
{
    public class UploadBlobFunction
    {
        private readonly ILogger<UploadBlobFunction> _logger;
        private readonly string _connectionString;

        public UploadBlobFunction(ILogger<UploadBlobFunction> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("StorageConnectionString")!;
        }

        [Function("UploadBlob")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "blob/upload")]
            HttpRequestData req)
        {
            _logger.LogInformation("UploadBlob function triggered.");

            try
            {
                string? fileName = System.Web.HttpUtility
                    .ParseQueryString(req.Url.Query)["fileName"];

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("A 'fileName' query string value is required.");
                    return bad;
                }

                var containerClient = new BlobContainerClient(_connectionString, "productimages");
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                string blobName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{fileName}";
                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.UploadAsync(req.Body, overwrite: true);

                _logger.LogInformation("Blob {BlobName} uploaded to product-images.", blobName);

                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync($"Image uploaded successfully. Blob URI: {blobClient.Uri}");
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload blob.");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Error uploading image: {ex.Message}");
                return error;
            }
        }
    }
}