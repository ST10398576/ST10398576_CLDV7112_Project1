using System.Text;
using System.Text.Json;

namespace ST10398576_CLDV7112_Project1.Services
{
    public class FunctionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FunctionApiService> _logger;

        public FunctionApiService(HttpClient httpClient, IConfiguration configuration,
            ILogger<FunctionApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(configuration["FunctionApi:BaseUrl"]!);
            // The function key authorises every request to the Functions app
            _httpClient.DefaultRequestHeaders.Add("x-functions-key", configuration["FunctionApi:Key"]);
        }

        public async Task<bool> StoreTableEntityAsync(string tableName, Dictionary<string, string> fields)
        {
            var json = new StringContent(JsonSerializer.Serialize(fields),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"table/{tableName}", json);
            await LogOutcomeAsync(response, $"StoreTableEntity ({tableName})");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UploadBlobAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var content = new StreamContent(stream);

            var response = await _httpClient.PostAsync(
                $"blob/upload?fileName={Uri.EscapeDataString(file.FileName)}", content);

            await LogOutcomeAsync(response, "UploadBlob");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendQueueMessageAsync(string message)
        {
            var content = new StringContent(message, Encoding.UTF8, "text/plain");

            var response = await _httpClient.PostAsync("queue/send", content);
            await LogOutcomeAsync(response, "WriteToQueue");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UploadLogFileAsync(string fileName, string contents)
        {
            var content = new StringContent(contents, Encoding.UTF8, "text/plain");

            var response = await _httpClient.PostAsync(
                $"files/upload?fileName={Uri.EscapeDataString(fileName)}", content);

            await LogOutcomeAsync(response, "UploadToFileShare");
            return response.IsSuccessStatusCode;
        }

        private async Task LogOutcomeAsync(HttpResponseMessage response, string functionName)
        {
            string body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("{Function} succeeded: {Body}", functionName, body);
            else
                _logger.LogError("{Function} failed ({Status}): {Body}",
                    functionName, response.StatusCode, body);
        }
    }
}