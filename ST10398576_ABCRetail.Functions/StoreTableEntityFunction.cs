using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ST10398576_ABCRetail.Functions
{
    public class StoreTableEntityFunction
    {
        private readonly ILogger<StoreTableEntityFunction> _logger;
        private readonly string _connectionString;

        public StoreTableEntityFunction(ILogger<StoreTableEntityFunction> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("StorageConnectionString")!;
        }

        [Function("StoreTableEntity")]
        public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "table/{tableName}")]
        HttpRequestData req, string tableName)
        {
            _logger.LogInformation("StoreTableEntity triggered for table {TableName}.", tableName);

            try
            {
                if (!string.Equals(tableName, "CustomerProfiles", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(tableName, "Products", StringComparison.OrdinalIgnoreCase))
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("Table must be either 'CustomerProfiles' or 'Products'.");
                    return bad;
                }

                string actualTableName = string.Equals(tableName, "CustomerProfiles", StringComparison.OrdinalIgnoreCase)
                    ? "customerProfiles"
                    : "Products";

                string body = await new StreamReader(req.Body).ReadToEndAsync();
                var fields = JsonSerializer.Deserialize<Dictionary<string, string>>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fields is null || !fields.ContainsKey("RowKey"))
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("A 'RowKey' value is required in the request body.");
                    return bad;
                }

                var tableClient = new TableClient(_connectionString, actualTableName);
                await tableClient.CreateIfNotExistsAsync();

                var entity = new TableEntity(
                    fields.GetValueOrDefault("PartitionKey", tableName.ToUpperInvariant()),
                    fields["RowKey"]);

                foreach (var field in fields.Where(f => f.Key != "PartitionKey" && f.Key != "RowKey"))
                {
                    entity[field.Key] = field.Value;
                }

                entity["CreatedOn"] = DateTime.UtcNow;

                await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);

                _logger.LogInformation("Entity {RowKey} written to {TableName}.", fields["RowKey"], tableName);

                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync($"Record '{fields["RowKey"]}' stored successfully in table '{tableName}'.");
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write entity to Azure Table Storage.");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync($"Error storing record: {ex.Message}");
                return error;
            }
        }
    }
}