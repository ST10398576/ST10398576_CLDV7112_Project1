using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Models;
using ST10398576_CLDV7112_Project1.Services;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    public class ProductController : Controller
    {
        private readonly ITableStorageService _tableService;
        private readonly FunctionApiService _functionApi;

        public ProductController(ITableStorageService tableService, FunctionApiService functionApi)
        {
            _tableService = tableService;
            _functionApi = functionApi;
        }

        // GET: /Product/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _tableService.GetAllProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load products: " + ex.Message;
                return View(new List<Product>());
            }
        }

        // POST: /Product/Add
        [HttpPost]
        public async Task<IActionResult> Add(Product product)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please complete all required product fields.";
                return RedirectToAction("Index");
            }

            // The product name is used as the RowKey so each catalogue item is unique
            var fields = new Dictionary<string, string>
            {
                { "PartitionKey", "PRODUCT" },
                { "RowKey", product.ProductName },
                { "Name", product.ProductName },
                { "Description", product.Description },
                { "Price", product.Price.ToString() },
                { "Stock", product.StockQuantity.ToString() }
            };

            bool success = await _functionApi.StoreTableEntityAsync("Products", fields);

            TempData["Message"] = success
                ? $"Product '{product.ProductName}' added via Azure Function."
                : "The function call failed. Please try again.";

            return RedirectToAction("Index");
        }
    }
}