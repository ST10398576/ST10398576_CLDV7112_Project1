using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Models;
using ST10398576_CLDV7112_Project1.Services;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ITableStorageService _tableService;

        public CustomerController(ITableStorageService tableService)
        {
            _tableService = tableService;
        }

        // GET: /Customer/Index
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetAllCustomersAsync();
            return View(customers);
        }

        // POST: /Customer/Add
        [HttpPost]
        public async Task<IActionResult> Add(CustomerProfile customer)
        {
            await _tableService.AddCustomerAsync(customer);
            return RedirectToAction("Index");
        }
    }
}