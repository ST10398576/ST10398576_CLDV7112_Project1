using System.Net;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ST10398576_ABCRetail.Functions
{
    public class UploadToFileShareFunction
    {
        private readonly ILogger<UploadToFileShareFunction> _logger;
        private readonly string _connectionString;

        public UploadToFileShareFunction(ILogger<UploadToFileShareFunction> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("StorageConnectionString")!;
        }

        [Function("UploadToFileShare")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "files/upload")]
            HttpRequestData req)
        {
            _logger.LogInformation("UploadToFileShare function triggered.");

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

                var shareClient = new ShareClient(_connectionString, "logfiles");
                await shareClient.CreateIfNotExistsAsync();

                var rootDirectory = shareClient.GetRootDirectoryClient();
                var fileClient = rootDirectory.GetFileClient(fileName);

                using var memoryStream = new MemoryStream();
                await req.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                await fileClient.CreateAsync(memoryStream.Length);
                await fileClient.UploadAsync(memoryStream);

                _logger.LogInformation("File {FileName} written to share 'contracts/payments'.", fileName);

                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync($"File '{fileName}' uploaded to Azure Files successfully.");
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to Azure Files.");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Error uploading file: {ex.Message}");
                return error;
            }
        }
    }
}