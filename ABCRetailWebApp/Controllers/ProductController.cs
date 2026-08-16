using Microsoft.AspNetCore.Mvc;
using ABCRetailWebApp.Services;
using ABCRetailWebApp.Models;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;

namespace ABCRetailWebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly AzureStorageService _storageService;

        public ProductController(AzureStorageService storageService)
        {
            _storageService = storageService;
        }

        // GET: Displays the page showing the upload form AND the list of products
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = new List<ProductViewModel>();

            try
            {
                // Pull all existing products from Azure Tables to display on the storefront storefront
                TableClient tableClient = _storageService.GetTableClient().GetTableClient("ProductInformation");
                var entities = tableClient.QueryAsync<ProductViewModel>(filter: $"PartitionKey eq 'RetailProduct'");

                await foreach (var entity in entities)
                {
                    products.Add(entity);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Could not load products: " + ex.Message;
            }

          
            ViewBag.Products = products;
            return View(new ProductViewModel());
        }

       
        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductViewModel model)
        {
          
            ModelState.Remove("PartitionKey");
            ModelState.Remove("RowKey");
            ModelState.Remove("Timestamp");
            ModelState.Remove("ETag");

            if (!ModelState.IsValid)
            {
                return await RebuildIndexView(model);
            }

            try
            {
                
                if (model.ProductImageFile != null && model.ProductImageFile.Length > 0)
                {
                    BlobServiceClient blobServiceClient = _storageService.GetBlobClient();
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("product-images");

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProductImageFile.FileName);
                    BlobClient blobClient = containerClient.GetBlobClient(uniqueFileName);

                    using (var stream = model.ProductImageFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                   
                    model.FinalImageUrl = blobClient.Uri.ToString();
                }

               
                TableClient tableClient = _storageService.GetTableClient().GetTableClient("ProductInformation");
                await tableClient.UpsertEntityAsync(model);

                ViewBag.Message = "Product successfully created and cataloged in the cloud inventory storage matrix!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to process product listing transaction sequence: " + ex.Message;
                return await RebuildIndexView(model);
            }
        }

        
        private async Task<IActionResult> RebuildIndexView(ProductViewModel model)
        {
            var products = new List<ProductViewModel>();
            try
            {
                TableClient tableClient = _storageService.GetTableClient().GetTableClient("ProductInformation");
                var entities = tableClient.QueryAsync<ProductViewModel>(filter: $"PartitionKey eq 'RetailProduct'");
                await foreach (var entity in entities) { products.Add(entity); }
            }
            catch { }
            ViewBag.Products = products;
            return View("Index", model);
        }
    }
}
