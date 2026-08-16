using Microsoft.AspNetCore.Mvc;
using ABCRetailWebApp.Services;
using ABCRetailWebApp.Models;
using Azure.Data.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ABCRetailWebApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AzureStorageService _storageService;

        public CustomerController(AzureStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = new List<CustomerProfileModel>();

            try
            {
             
                TableClient tableClient = _storageService.GetTableClient().GetTableClient("CustomerProfiles");

               
                var entities = tableClient.QueryAsync<CustomerProfileModel>(filter: "PartitionKey eq 'Customer'");

                await foreach (var entity in entities)
                {
                    customers.Add(entity);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Could not pull directory entries: " + ex.Message;
            }

          
            ViewBag.Customers = customers;
            return View(new CustomerProfileModel());
        }

      
        [HttpPost]
        public async Task<IActionResult> Create(CustomerProfileModel model)
        {
           
            ModelState.Remove("PartitionKey");
            ModelState.Remove("RowKey");
            ModelState.Remove("Timestamp");
            ModelState.Remove("ETag");

            if (!ModelState.IsValid)
            {
                return await RebuildCustomerView(model);
            }

            try
            {
                TableClient tableClient = _storageService.GetTableClient().GetTableClient("CustomerProfiles");
                await tableClient.UpsertEntityAsync(model);

                ViewBag.Message = "Customer profile saved successfully to Azure Tables!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Storage operation failed: " + ex.Message;
                return await RebuildCustomerView(model);
            }
        }

        private async Task<IActionResult> RebuildCustomerView(CustomerProfileModel model)
        {
            var customers = new List<CustomerProfileModel>();
            try
            {
                TableClient tableClient = _storageService.GetTableClient().GetTableClient("CustomerProfiles");
                var entities = tableClient.QueryAsync<CustomerProfileModel>(filter: "PartitionKey eq 'Customer'");
                await foreach (var entity in entities) { customers.Add(entity); }
            }
            catch { }
            ViewBag.Customers = customers;
            return View("Index", model);
        }
    }
}
