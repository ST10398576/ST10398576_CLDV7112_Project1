using System.Net;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ST10398576_ABCRetail.Functions
{
    public class QueueFunctions
    {
        private readonly ILogger<QueueFunctions> _logger;
        private readonly string _connectionString;
        private const string QueueName = "orderprocessing";

        public QueueFunctions(ILogger<QueueFunctions> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("StorageConnectionString")!;
        }

        [Function("WriteToQueue")]
        public async Task<HttpResponseData> WriteToQueue(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "queue/send")]
            HttpRequestData req)
        {
            _logger.LogInformation("WriteToQueue function triggered.");

            try
            {
                string message = await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(message))
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("A message body is required.");
                    return bad;
                }

                var options = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 };
                var queueClient = new QueueClient(_connectionString, QueueName, options);
                await queueClient.CreateIfNotExistsAsync();

                await queueClient.SendMessageAsync(message);

                _logger.LogInformation("Message placed on queue {QueueName}.", QueueName);

                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync($"Transaction message queued: {message}");
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write message to the queue.");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Error queueing message: {ex.Message}");
                return error;
            }
        }

        [Function("ProcessQueueMessage")]
        public void ProcessQueueMessage(
            [QueueTrigger(QueueName, Connection = "StorageConnectionString")] string message)
        {
            _logger.LogInformation("Queue message read and processed: {Message}", message);
        }
    }
}