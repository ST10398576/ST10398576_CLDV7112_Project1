using Microsoft.AspNetCore.Mvc;
using ST10398576_CLDV7112_Project1.Models;
using ST10398576_CLDV7112_Project1.Services;

namespace ST10398576_CLDV7112_Project1.Controllers
{
    public class ImageController : Controller
    {
        private readonly IBlobStorageService _blobService;
        private readonly IQueueStorageService _queueService;
        private readonly IFileStorageService _fileService;

        public ImageController(
            IBlobStorageService blobService,
            IQueueStorageService queueService,
            IFileStorageService fileService)
        {
            _blobService = blobService;
            _queueService = queueService;
            _fileService = fileService;
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
                var (stream, contentType, length) = await _blobService.DownloadBlobAsync(blobName);
                // Return as file stream result
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

        // GET: /Image/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var urls = await _blobService.ListBlobUrlsAsync();
                return View(urls ?? new System.Collections.Generic.List<string>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load images: " + ex.Message;
                return View(new System.Collections.Generic.List<string>());
            }
        }

        // POST: /Image/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction("Index");
            }

            try
            {
                string imageUrl = await _blobService.UploadFileAsync(imageFile);

                await _queueService.SendMessageAsync(
                    $"ImageUploaded: {imageFile.FileName} | Status: Processing order");

                await _fileService.WriteLogAsync(new LogEntry
                {
                    Action = "ImageUploaded",
                    Details = $"File '{imageFile.FileName}' uploaded to Blob Storage at {imageUrl}."
                });

                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                // Known operational errors (e.g. storage not configured)
                TempData["Error"] = "Upload failed: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // Generic catch - avoid leaking sensitive details
                TempData["Error"] = "An unexpected error occurred while uploading the image.";
                return RedirectToAction("Index");
            }
        }
    }
}