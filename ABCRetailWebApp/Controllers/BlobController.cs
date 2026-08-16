using Microsoft.AspNetCore.Mvc;
using ABCRetailWebApp.Services;
using ABCRetailWebApp.Models;
using Azure.Storage.Blobs;

namespace ABCRetailWebApp.Controllers
{
    public class BlobController : Controller
    {
        private readonly AzureStorageService _storageService;
        public BlobController(AzureStorageService storageService)
        {
            _storageService = storageService;
        }

        // GET Displays the upload page
        [HttpGet]
        public IActionResult Index()
        {
            return View(new BlobStorageModel());
        }

        // POST Handles the file upload to Azure
        [HttpPost]
        public async Task<IActionResult> Upload(BlobStorageModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Please select a valid image file first.");
                return View("Index", model);
            }

            try
            {
                BlobServiceClient blobServiceClient = _storageService.GetBlobClient();
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("product-images");

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                BlobClient blobClient = containerClient.GetBlobClient(uniqueFileName);

                using (var stream = model.ImageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                model.ImageUrl = blobClient.Uri.ToString();
                ViewBag.Message = "Image uploaded successfully to Azure!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Upload failed: " + ex.Message;
            }

            return View("Index", model);
        }
    }
}
