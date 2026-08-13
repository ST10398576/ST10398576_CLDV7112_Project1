using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Services;
using System.Text;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    public class LogController : Controller
    {
        private readonly IFileStorageService _fileService;

        public LogController(IFileStorageService fileService)
        {
            _fileService = fileService;
        }

        // GET: /Log/Index
        public async Task<IActionResult> Index()
        {
            var files = await _fileService.ListLogFileNamesAsync();
            return View(files);
        }

        // GET: /Log/View?fileName=...
        // Displays the log file content inline in the browser.
        public async Task<IActionResult> View(string fileName)
        {
            string content = await _fileService.ReadLogAsync(fileName);
            ViewBag.FileName = fileName;
            ViewBag.Content = content;
            return base.View();
        }

        // GET: /Log/Download?fileName=...
        // Forces the browser to download the log file as an attachment,
        // satisfying the brief's "upload or download" control requirement.
        public async Task<IActionResult> Download(string fileName)
        {
            string content = await _fileService.ReadLogAsync(fileName);
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            return File(bytes, "text/plain", fileName);
        }
    }
}