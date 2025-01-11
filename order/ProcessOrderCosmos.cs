using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.Azure.Storage.Blob;
using Azure.Storage.Blobs;

namespace AzureCourse.Function
{
    public class CosmosOrderFunction
    {
        private readonly ILogger<CosmosOrderFunction> _logger;

        public CosmosOrderFunction(ILogger<CosmosOrderFunction> logger)
        {
            _logger = logger;
        }

        [Function("ProcessOrderCosmos")]
        [CosmosDBOutput(databaseName: "readit-orders", containerName: "orders", Connection = "CosmosDBConnection", CreateIfNotExists = true)]
        public object Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [BlobInput("neworders", Connection = "StorageConnectionString")] BlobContainerClient container)
        {
            _logger.LogInformation($"ProcessOrderCosmos called: {Guid.NewGuid()}");
            try
            {
                _logger.LogInformation("Function started");
                _logger.LogInformation($"Event details: Topic: {eventGridEvent.Topic}");
                _logger.LogInformation($"Event data: {eventGridEvent.Data.ToString()}");

                string eventBody = eventGridEvent.Data.ToString();

                _logger.LogInformation("Deserializing to StorageBlobCreatedEventData...");
                var storageData = JsonSerializer.Deserialize<StorageBlobCreatedEventData>(eventBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                _logger.LogInformation("Done");

                _logger.LogInformation("Get the name of the new blob...");
                var blobName = Path.GetFileName(storageData.Url);
                _logger.LogInformation($"Name of file: {blobName}");

                _logger.LogInformation("Get blob from storage...");
                var blockBlob = container.GetBlobClient(blobName);
                var orderText = blockBlob.DownloadContent().Value.Content.ToString();
                _logger.LogInformation("Done");
                _logger.LogInformation($"Order text: {orderText}");

                var order = JsonSerializer.Deserialize<Order>(orderText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Order();
                order.id = Guid.NewGuid().ToString();
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in function");
                return null;
            }
        }
    }
}

