using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Services;
using System.IO;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    public class ImageController : Controller
    {
        private readonly IBlobStorageService _blobService;
        private readonly FunctionApiService _functionApi;

        public ImageController(IBlobStorageService blobService, FunctionApiService functionApi)
        {
            _blobService = blobService;
            _functionApi = functionApi;
        }

        // GET: /Image/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var urls = await _blobService.ListBlobUrlsAsync();
                return View(urls ?? new List<string>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load images: " + ex.Message;
                return View(new List<string>());
            }
        }

        // GET: /Image/Download?blobName=...
        public async Task<IActionResult> Download(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return BadRequest();
            }

            try
            {
                (Stream stream, string contentType, long length) =
                    await _blobService.DownloadBlobAsync(blobName);

                return File(stream, contentType, fileDownloadName: Path.GetFileName(blobName));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = "Download failed: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "An unexpected error occurred while downloading the image.";
                return RedirectToAction("Index");
            }
        }

        // POST: /Image/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                TempData["Error"] = "Please select an image to upload.";
                return RedirectToAction("Index");
            }

            try
            {
                bool blobOk = await _functionApi.UploadBlobAsync(file);

                bool queueOk = await _functionApi.SendQueueMessageAsync(
                    $"IMAGE_UPLOADED|{file.FileName}|{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

                string logName = $"ImageUploaded_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.txt";
                bool fileOk = await _functionApi.UploadLogFileAsync(logName,
                    $"Image '{file.FileName}' ({file.Length} bytes) uploaded at {DateTime.UtcNow:u}.");

                TempData["Message"] = (blobOk && queueOk && fileOk)
                    ? "Image uploaded, message queued and log file written via Azure Functions."
                    : "One or more function calls failed. Check the application logs.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Upload failed: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}