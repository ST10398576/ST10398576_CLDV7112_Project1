using Azure.Data.Tables;
using ST10398576_CLDV7112_Project1.Models;

namespace ST10398576_CLDV7112_Project1.Services
{
    public interface ITableStorageService
    {
        Task AddCustomerAsync(CustomerProfile customer);
        Task<List<CustomerProfile>> GetAllCustomersAsync();

        Task AddProductAsync(Product product);
        Task<List<Product>> GetAllProductsAsync();
    }

    public class TableStorageService : ITableStorageService
    {
        private readonly TableClient _customerTable;
        private readonly TableClient _productTable;

        public TableStorageService(IConfiguration config)
        {
            string connectionString = config.GetValue<string>("AzureStorage:ConnectionString")!;

            var serviceClient = new TableServiceClient(connectionString);

            _customerTable = serviceClient.GetTableClient("CustomerProfiles");
            _customerTable.CreateIfNotExists();

            _productTable = serviceClient.GetTableClient("Products");
            _productTable.CreateIfNotExists();
        }

        public async Task AddCustomerAsync(CustomerProfile customer)
        {
            await _customerTable.AddEntityAsync(customer);
        }

        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var results = new List<CustomerProfile>();
            await foreach (var entity in _customerTable.QueryAsync<CustomerProfile>())
            {
                results.Add(entity);
            }
            return results;
        }

        public async Task AddProductAsync(Product product)
        {
            await _productTable.AddEntityAsync(product);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var results = new List<Product>();
            await foreach (var entity in _productTable.QueryAsync<Product>())
            {
                results.Add(entity);
            }
            return results;
        }
    }
}