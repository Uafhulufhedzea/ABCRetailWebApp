using Microsoft.AspNetCore.Mvc;
using ABCRetailWebApp.Services;
using ABCRetailWebApp.Models;
using Azure.Storage.Files.Shares;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System;

namespace ABCRetailWebApp.Controllers
{
    public class LogController : Controller
    {
        private readonly AzureStorageService _storageService;

        public LogController(AzureStorageService storageService)
        {
            _storageService = storageService;
        }

        // GET: Displays the logging form AND reads existing log files from the cloud
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                ShareServiceClient shareServiceClient = _storageService.GetFileClient();
                ShareClient shareClient = shareServiceClient.GetShareClient("application-logs");
                ShareDirectoryClient rootDirectoryClient = shareClient.GetRootDirectoryClient();
                ShareFileClient fileClient = rootDirectoryClient.GetFileClient("app-log.txt");

                if (await fileClient.ExistsAsync())
                {
                    var downloadInfo = await fileClient.DownloadAsync();
                    using (var reader = new StreamReader(downloadInfo.Value.Content, Encoding.UTF8))
                    {
                        ViewBag.LogHistory = await reader.ReadToEndAsync();
                    }
                }
                else
                {
                    ViewBag.LogHistory = "No active system log metrics found in cloud file share repository boundaries.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.LogHistory = "Failed to stream live diagnostics content: " + ex.Message;
            }

            return View(new LogFileModel());
        }

        // POST: Uploads/Writes log data to Azure Files
        [HttpPost]
        public async Task<IActionResult> WriteLog(LogFileModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                ShareServiceClient shareServiceClient = _storageService.GetFileClient();
                ShareClient shareClient = shareServiceClient.GetShareClient("application-logs");
                ShareDirectoryClient rootDirectoryClient = shareClient.GetRootDirectoryClient();
                ShareFileClient fileClient = rootDirectoryClient.GetFileClient(model.FileName);

                // ✅ Forces explicit line breaks that both Azure Files and HTML <pre> tags recognize
                string newEntry = model.LogContent + Environment.NewLine;

                string fullContent;
                if (await fileClient.ExistsAsync())
                {
                    var downloadInfo = await fileClient.DownloadAsync();
                    using (var reader = new StreamReader(downloadInfo.Value.Content, Encoding.UTF8))
                    {
                        fullContent = await reader.ReadToEndAsync();
                    }
                    fullContent += newEntry;
                }
                else
                {
                    fullContent = newEntry;
                }

                // Convert to bytes and upload using MemoryStream
                byte[] bytes = Encoding.UTF8.GetBytes(fullContent);
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    stream.Position = 0;

                    // Ensure file is recreated with the correct size then upload the single range
                    if (await fileClient.ExistsAsync())
                    {
                        await fileClient.DeleteAsync();
                    }

                    await fileClient.CreateAsync(bytes.Length);
                    await fileClient.UploadRangeAsync(new Azure.HttpRange(0, bytes.Length), stream);
                }

                ViewBag.Message = $"Successfully written log data to '{model.FileName}' in Azure Files!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "File storage operation failed: " + ex.Message;
                return View("Index", model);
            }
        }
    }
}
