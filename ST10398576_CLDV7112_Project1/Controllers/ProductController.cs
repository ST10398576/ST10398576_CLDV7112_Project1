using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Models;
using ST10398576_CLDV7112_Project1.Services;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    public class ProductController : Controller
    {
        private readonly ITableStorageService _tableService;

        public ProductController(ITableStorageService tableService)
        {
            _tableService = tableService;
        }

        // GET: /Product/Index
        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetAllProductsAsync();
            return View(products);
        }

        // POST: /Product/Add
        [HttpPost]
        public async Task<IActionResult> Add(Product product)
        {
            await _tableService.AddProductAsync(product);
            return RedirectToAction("Index");
        }
    }
}