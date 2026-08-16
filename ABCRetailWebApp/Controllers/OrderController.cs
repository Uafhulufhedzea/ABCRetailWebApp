using Microsoft.AspNetCore.Mvc;
using ABCRetailWebApp.Services;
using ABCRetailWebApp.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ABCRetailWebApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly AzureStorageService _storageService;

        public OrderController(AzureStorageService storageService)
        {
            _storageService = storageService;
        }

        // GET: Displays the message form AND lists existing messages in the queue
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var queueMessages = new List<CustomQueueMessage>();

            try
            {
                QueueClient queueClient = _storageService.GetQueueClient().GetQueueClient("order-processing");

                // Peek up to 20 messages currently sitting inside the cloud queue
                PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 20);

                foreach (var msg in peekedMessages)
                {
                    queueMessages.Add(new CustomQueueMessage
                    {
                        MessageId = msg.MessageId,
                        BodyText = msg.MessageText
                    });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Could not access message queue bounds: " + ex.Message;
            }

            ViewBag.QueueMessages = queueMessages;
            return View(new OrderMessageModel());
        }

        // POST: Sends a new message down into the Azure Queue matrix
        [HttpPost]
        public async Task<IActionResult> Send(OrderMessageModel model)
        {
            if (!ModelState.IsValid)
            {
                return await RebuildOrderView(model);
            }

            try
            {
                QueueClient queueClient = _storageService.GetQueueClient().GetQueueClient("order-processing");
                await queueClient.SendMessageAsync(model.Content);

                ViewBag.Message = "Order message successfully placed into Azure Queue!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Queue operation failed: " + ex.Message;
                return await RebuildOrderView(model);
            }
        }

        // POST: Dequeues and deletes a completed processing event record
        [HttpPost]
        public async Task<IActionResult> ProcessMessage(string messageId)
        {
            try
            {
                QueueClient queueClient = _storageService.GetQueueClient().GetQueueClient("order-processing");

                // Fetch response envelope containing raw messages from Azure storage
                var receivedMessagesResponse = await queueClient.ReceiveMessagesAsync(maxMessages: 32);
                var receivedMessages = receivedMessagesResponse.Value;

                foreach (var msg in receivedMessages)
                {
                    if (msg.MessageId == messageId)
                    {
                        // Safely purge it from the live cloud queue
                        await queueClient.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
                        ViewBag.Message = "Order processed successfully and removed from queue infrastructure!";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Fulfillment processing pipeline failed: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private async Task<IActionResult> RebuildOrderView(OrderMessageModel model)
        {
            var queueMessages = new List<CustomQueueMessage>();
            try
            {
                QueueClient queueClient = _storageService.GetQueueClient().GetQueueClient("order-processing");
                PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 20);
                foreach (var msg in peekedMessages)
                {
                    queueMessages.Add(new CustomQueueMessage { MessageId = msg.MessageId, BodyText = msg.MessageText });
                }
            }
            catch { }
            ViewBag.QueueMessages = queueMessages;
            return View("Index", model);
        }
    }

    // Explicitly renamed helper class to avoid naming conflicts with Azure SDK classes
    public class CustomQueueMessage
    {
        public string MessageId { get; set; } = string.Empty;
        public string BodyText { get; set; } = string.Empty;
    }
}
