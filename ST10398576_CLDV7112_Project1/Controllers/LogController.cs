using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Services;

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
        public async Task<IActionResult> View(string fileName)
        {
            string content = await _fileService.ReadLogAsync(fileName);
            ViewBag.FileName = fileName;
            ViewBag.Content = content;
            return base.View();
        }
    }
}