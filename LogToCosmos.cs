using System.Text;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ProcessLogs
{
    public class LogToCosmos
    {
        private readonly ILogger<LogToCosmos> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public LogToCosmos(ILogger<LogToCosmos> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer(
                Environment.GetEnvironmentVariable("CosmosDBDatabaseName"),
                Environment.GetEnvironmentVariable("CosmosDBContainerName")
            );
        }

        [Function(nameof(LogToCosmos))]
        public async Task Run(
            [EventHubTrigger("%EventHubName%", Connection = "EventHubsConnectionString")] EventData[] events,
            FunctionContext context)
        {            
            foreach (EventData eventData in events)
            {
                // Convert the EventData to a json object
                var eventJson = JObject.Parse(Encoding.UTF8.GetString(eventData.EventBody));

                // Add each element from the json object to a dictionary
                // that will be used to create a Cosmos DB document
                var properties = new Dictionary<string, object>();
                foreach (var property in eventJson.Properties())
                {
                    properties[property.Name] = property.Value;
                }

                // Create a Cosmos DB document with the properties
                var documentId = Guid.NewGuid().ToString();
                var document = new Dictionary<string, object>
                {
                    { "id", documentId }
                };

                foreach (var property in properties)
                {
                    document[property.Key] = property.Value;
                }

                // Write the document to Cosmos DB
                await _container.CreateItemAsync(document);                
            }
        }
    }
}
