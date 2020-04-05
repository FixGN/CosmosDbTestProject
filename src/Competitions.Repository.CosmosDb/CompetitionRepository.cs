using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Cosmos;
using Competitions.Models;
using Competitions.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Competitions.Repository.CosmosDb
{
    public class CompetitionRepository : ICompetitionRepository
    {
        private readonly CosmosClient _client;
        private readonly CosmosDatabase _database;
        private readonly CosmosContainer _container;
        private readonly ILogger<CompetitionRepository> _logger;

        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public CompetitionRepository(
            IOptions<CosmosDbConfiguration> options,
            ILogger<CompetitionRepository> logger)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            var configuration = options.Value;
            
            _logger = logger;
            _client = new CosmosClient(configuration.Uri, configuration.MasterKey);
            _database = ConnectToDatabase(
                    configuration.DatabaseId, 
                    configuration.DatabaseThroughput)
                .Result;
            _container =
                ConnectToContainer(
                    configuration.ContainerId,
                    $"/{nameof(Competition.Location)}/{nameof(Location.State)}")
                .Result;
        }

        public async Task InsertCompetition(Competition competition)
        {
            if (competition == null)
                throw new ArgumentNullException(nameof(competition));

            try
            {
                var itemResponse = await _container
                    .CreateItemAsync(competition, new PartitionKey(competition.Location.State));
                _logger.LogInformation($"Competition with id '{itemResponse.Value.Id}' and " +
                                       $"title '{itemResponse.Value.Title}' added successful");

                var isHaveHeaderRequestCharge = itemResponse
                    .GetRawResponse()
                    .Headers
                    .TryGetValue("x-ms-request-charge", out var requestUnits); 
                
                if (isHaveHeaderRequestCharge)
                    _logger.LogInformation($"Charged request units: {requestUnits}");
            }
            catch (CosmosException e) when (e.Response.Status == (int) HttpStatusCode.Conflict)
            {
                _logger.LogWarning($"Item with id '{competition.Id}' already exists!");
                
                var requestUnitHeader = GetRequestUnitHeader(e.Response.Headers);
                if (requestUnitHeader != null)
                    _logger.LogWarning($"Charged request units: {requestUnitHeader}");
            }
        }

        public async Task<Competition[]> GetCompetitionsByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("Try to get competitions with null or " +
                                   "empty string as title. Skip select...");
                return null;
            }
             
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.{nameof(Competition.Title)} = '{title}'");
            var requestedItemPages = await _container
                .GetItemQueryIterator<Competition>(query)
                .AsPages()
                .FirstOrDefaultAsync();
        
            var items = requestedItemPages.Values.Where(x => x != null).ToArray();
             
            _logger.LogInformation(items.Length == 0 
                ? $"Items with title '{title}' not found"
                : $"Item with title '{title}' receive successful. Count: {items.Length}");
        
            var requestUnit = GetRequestUnitHeader(requestedItemPages.GetRawResponse().Headers);
             
            if (requestUnit != null)
                _logger.LogInformation($"Charged request units: {requestUnit}");
        
            return items.Length == 0 ? null : items;
        }

        public async Task<int> GetCountCompetitionsByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("Try to get competitions count with null or " +
                                   "empty string as title. Skip select...");
                return 0;
            }
             
            var query = new QueryDefinition($"SELECT VALUE COUNT(1) FROM c WHERE c.{nameof(Competition.Title)} = '{title}'");
            var requestedItemPages = await _container
                .GetItemQueryIterator<int>(query)
                .AsPages()
                .FirstOrDefaultAsync();

            var item = requestedItemPages.Values.FirstOrDefault();
             
            _logger.LogInformation(item == 0 
                ? $"Items with title '{title}' not found"
                : $"Found {item} items with title '{title}'");
        
            var requestUnits = GetRequestUnitHeader(requestedItemPages.GetRawResponse().Headers);
             
            if (requestUnits != null)
                _logger.LogInformation($"Charged request units: {requestUnits}");
        
            return requestedItemPages.Values.FirstOrDefault();
        }

        public async Task UpdateDatetimeAndNumberOfCompetitionsByTitleAndState(
            string title,
            string state,
            DateTime newDatetime,
            int newNumberOfCompetitors)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException($"{nameof(title)} is null or empty");
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException($"{nameof(state)} is null or empty");
            if (newDatetime == null)
                throw new ArgumentNullException(nameof(newDatetime));
            
            var requestedItemResponse = await _container
                .ReadItemAsync<Competition>(title, new PartitionKey(state));
            if (requestedItemResponse.Value == null)
                throw new ApplicationException($"Item with title '{title}' and state '{state}' not found...");

            _logger.LogInformation($"Item with title '{title}' and state '{state}' found");

            var requestUnits = GetRequestUnitHeader(requestedItemResponse.GetRawResponse().Headers);
            if (requestUnits != null)
                _logger.LogInformation($"Charged request units: {requestUnits}");

            var originalItem = requestedItemResponse.Value;
            originalItem.DateTime = newDatetime;
            originalItem.NumberOfCompetitors = newNumberOfCompetitors;

            var replaceItemResponse = await _container
                .ReplaceItemAsync(originalItem, title, new PartitionKey(state));
            
            if (requestedItemResponse.Value == null)
                throw new ApplicationException($"Item with title '{title}' and state '{state}' not found...");

            _logger.LogInformation($"Item with title '{title}' and state '{state}' replaced");
            
            requestUnits = GetRequestUnitHeader(replaceItemResponse.GetRawResponse().Headers);
            if (requestUnits != null)
                _logger.LogInformation($"Charged request units: {requestUnits}");
        }

        public async Task<Competition[]> GetScheduledCompetitionWithManyCompetitors(int competitors)
        {
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.{nameof(Competition.NumberOfCompetitors)} > {competitors} " +
                                            $"AND c.{nameof(Competition.Status)} = '{CompetitionStatus.Scheduled}'");
            var requestedItemPages = await _container
                .GetItemQueryIterator<Competition>(query)
                .AsPages()
                .FirstOrDefaultAsync();
        
            var items = requestedItemPages.Values.Where(x => x != null).ToArray();
            
            _logger.LogInformation(items.Length == 0 
                ? $"Scheduled competitions with c.{nameof(Competition.NumberOfCompetitors)} > '{competitors}' not found"
                : $"Scheduled competitions with c.{nameof(Competition.NumberOfCompetitors)} > '{competitors}' found successful. Count: {items.Length}");

            var requestUnit = GetRequestUnitHeader(requestedItemPages.GetRawResponse().Headers);
             
            if (requestUnit != null)
                _logger.LogInformation($"Charged request units: {requestUnit}");
            
            return items;
        }

        private async Task<CosmosDatabase> ConnectToDatabase(string databaseId, int databaseThroughput)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
                throw new ApplicationException($"{nameof(databaseId)} can't be null or empty");
            
            var databaseResponse = await _client
                .CreateDatabaseIfNotExistsAsync(databaseId, databaseThroughput);
            _logger.LogInformation($"Connected to container with id {databaseId}");
            
            return databaseResponse.Database;
        }
        
        private async Task<CosmosContainer> ConnectToContainer(string containerId, string partitionKeyPath)
        {
            if (string.IsNullOrWhiteSpace(containerId))
                throw new ApplicationException($"{nameof(containerId)} can't be null or empty");

            var containerResponse = await _database.DefineContainer(containerId, partitionKeyPath)
                .WithUniqueKey()
                .Path($"/{nameof(Competition.Title)}")
                .Attach()
                .CreateIfNotExistsAsync();
            
            _logger.LogInformation($"Connected to container with id {containerId}");

            return containerResponse.Container;
        }

        private static float? GetRequestUnitHeader(ResponseHeaders headers)
        {
            var isHaveHeaderRequestCharge = headers.TryGetValue("x-ms-request-charge", out var requestUnits );

            if (!isHaveHeaderRequestCharge) return null;
            
            var isFloat = float.TryParse(requestUnits, out var result);
            if (isFloat)
                return result;

            return null;
        }
    }
}