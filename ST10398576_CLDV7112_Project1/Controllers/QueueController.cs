using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Services;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    public class QueueController : Controller
    {
        private readonly IQueueStorageService _queueService;

        public QueueController(IQueueStorageService queueService)
        {
            _queueService = queueService;
        }

        // GET: /Queue/Index
        public async Task<IActionResult> Index()
        {
            var messages = await _queueService.PeekMessagesAsync();
            return View(messages);
        }

        // POST: /Queue/Send
        [HttpPost]
        public async Task<IActionResult> Send(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                await _queueService.SendMessageAsync(message);
            }
            return RedirectToAction("Index");
        }
    }
}