using Azure.Data.Tables;
using ST10398576_CLDV7112_Project1.Models;

namespace ST10398576_CLDV7112_Project1.Services
{
    public interface ITableStorageService
    {
        Task AddCustomerAsync(CustomerProfile customer);
        Task<List<CustomerProfile>> GetAllCustomersAsync();
    }

    public class TableStorageService : ITableStorageService
    {
        private readonly TableClient? _customerTable;
        private readonly bool _initialized;

        public TableStorageService(IConfiguration config)
        {
            try
            {
                string? connectionString = config.GetValue<string>("AzureStorage:ConnectionString");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _initialized = false;
                    return;
                }

                var serviceClient = new TableServiceClient(connectionString);

                _customerTable = serviceClient.GetTableClient("CustomerProfiles");
                _customerTable.CreateIfNotExists();

                _initialized = true;
            }
            catch
            {
                _initialized = false;
            }
        }

        public async Task AddCustomerAsync(CustomerProfile customer)
        {
            if (!_initialized || _customerTable == null) return;
            await _customerTable.AddEntityAsync(customer);
        }

        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var results = new List<CustomerProfile>();
            if (!_initialized || _customerTable == null) return results;

            await foreach (var entity in _customerTable.QueryAsync<CustomerProfile>())
            {
                results.Add(entity);
            }
            return results;
        }
    }
}