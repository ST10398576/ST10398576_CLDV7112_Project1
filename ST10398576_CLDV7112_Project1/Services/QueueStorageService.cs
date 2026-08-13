using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace ST10398576_CLDV7112_Project1.Services
{
    public interface IQueueStorageService
    {
        Task SendMessageAsync(string message);
        Task<List<string>> PeekMessagesAsync(int maxMessages = 10);
    }

    public class QueueStorageService : IQueueStorageService
    {
        private readonly QueueClient? _queueClient;
        private readonly bool _initialized;

        public QueueStorageService(IConfiguration config)
        {
            try
            {
                string? connectionString = config.GetValue<string>("AzureStorage:ConnectionString");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _initialized = false;
                    return;
                }

                string queueName = "orderprocessing";

                _queueClient = new QueueClient(connectionString, queueName);
                _queueClient.CreateIfNotExists();
                _initialized = true;
            }
            catch
            {
                _initialized = false;
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (!_initialized || _queueClient == null) return;
            await _queueClient.SendMessageAsync(message);
        }

        public async Task<List<string>> PeekMessagesAsync(int maxMessages = 10)
        {
            var results = new List<string>();
            if (!_initialized || _queueClient == null) return results;

            PeekedMessage[] messages = await _queueClient.PeekMessagesAsync(maxMessages);

            foreach (var msg in messages)
            {
                results.Add(msg.MessageText);
            }
            return results;
        }
    }
}