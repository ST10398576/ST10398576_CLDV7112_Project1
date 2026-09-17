using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Services;
using System.Text;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    public class LogController : Controller
    {
        private readonly IFileStorageService _fileService;
        private readonly FunctionApiService _functionApi;

        public LogController(IFileStorageService fileService, FunctionApiService functionApi)
        {
            _fileService = fileService;
            _functionApi = functionApi;
        }

        // GET: /Log/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var files = await _fileService.ListLogFileNamesAsync();
                return View(files);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load log files: " + ex.Message;
                return View(new List<string>());
            }
        }

        // POST: /Log/Create
        [HttpPost]
        public async Task<IActionResult> Create(string note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                TempData["Error"] = "Please enter a note to log.";
                return RedirectToAction("Index");
            }

            string fileName = $"ManualLog_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.txt";
            bool success = await _functionApi.UploadLogFileAsync(fileName,
                $"{DateTime.UtcNow:u} - {note}");

            TempData["Message"] = success
                ? $"Log file '{fileName}' written to Azure Files via Azure Function."
                : "The function call failed. Please try again.";

            return RedirectToAction("Index");
        }

        // GET: /Log/View?fileName=...
        public async Task<IActionResult> View(string fileName)
        {
            string content = await _fileService.ReadLogAsync(fileName);
            ViewBag.FileName = fileName;
            ViewBag.Content = content;
            return base.View();
        }

        // GET: /Log/Download?fileName=...
        public async Task<IActionResult> Download(string fileName)
        {
            string content = await _fileService.ReadLogAsync(fileName);
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            return File(bytes, "text/plain", fileName);
        }
    }
}