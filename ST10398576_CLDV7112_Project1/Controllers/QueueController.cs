using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Services;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    public class QueueController : Controller
    {
        private readonly IQueueStorageService _queueService;
        private readonly FunctionApiService _functionApi;

        public QueueController(IQueueStorageService queueService, FunctionApiService functionApi)
        {
            _queueService = queueService;
            _functionApi = functionApi;
        }

        // GET: /Queue/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var messages = await _queueService.PeekMessagesAsync();
                return View(messages);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to read queue messages: " + ex.Message;
                return View(new List<string>());
            }
        }

        // POST: /Queue/Send
        [HttpPost]
        public async Task<IActionResult> Send(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Please enter a message to send.";
                return RedirectToAction("Index");
            }

            // The Azure Function places the message on the 'orderprocessing' queue
            bool success = await _functionApi.SendQueueMessageAsync(message);

            TempData["Message"] = success
                ? "Message sent to the queue via Azure Function."
                : "The function call failed. Please try again.";

            return RedirectToAction("Index");
        }
    }
}