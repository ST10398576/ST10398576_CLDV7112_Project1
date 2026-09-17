using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Models;
using ST10398576_CLDV7112_Project1.Services;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    /// <summary>
    /// Manages customer profiles. Writes are delegated to the Azure Functions app
    /// (Project 2), while reads continue to use the table service directly.
    /// </summary>
    public class CustomerController : Controller
    {
        private readonly ITableStorageService _tableService;
        private readonly FunctionApiService _functionApi;

        public CustomerController(ITableStorageService tableService, FunctionApiService functionApi)
        {
            _tableService = tableService;
            _functionApi = functionApi;
        }

        // GET: /Customer/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _tableService.GetAllCustomersAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load customers: " + ex.Message;
                return View(new List<CustomerProfile>());
            }
        }

        // POST: /Customer/AddCustomer
        [HttpPost]
        public async Task<IActionResult> AddCustomer(CustomerProfile customer)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please complete all required customer fields.";
                return RedirectToAction("Index");
            }

            // Build the flat key/value payload expected by the StoreTableEntity function
            var fields = new Dictionary<string, string>
            {
                { "PartitionKey", "CUSTOMER" },
                { "RowKey", customer.Email },
                { "FullName", customer.FullName },
                { "Email", customer.Email },
                { "PhoneNumber", customer.PhoneNumber },
                { "ShippingAddress", customer.ShippingAddress }
            };

            // The Azure Function performs the actual write to Azure Table Storage
            bool success = await _functionApi.StoreTableEntityAsync("CustomerProfiles", fields);

            TempData["Message"] = success
                ? $"Customer '{customer.FullName}' added via Azure Function."
                : "The function call failed. Please try again.";

            return RedirectToAction("Index");
        }
    }
}