using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using BeerMonitor.Interfaces;
using BeerMonitor.BusinessLogic;

namespace BeerMonitor.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private CloudTable _beerTable;
        private TelemetryClient telemetry = new TelemetryClient();

        public BlobStorageService()
        {
            StorageCredentials creds = new StorageCredentials(ConfigurationManager.BlobName, ConfigurationManager.BlobKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(creds, true);
            var client = storageAccount.CreateCloudTableClient();
            _beerTable = client.GetTableReference(ConfigurationManager.BlobTableName);
        }

        public async Task<bool> InsertTempAndHumidity(double temp, double humidity)
        {
            try
            {
                await _beerTable.CreateIfNotExistsAsync();
                var insertOperation = TableOperation.Insert(new BeerTempAndHumidity(temp, humidity, DateTime.Now));
                await _beerTable.ExecuteAsync(insertOperation);
                return true;
            }
            catch(StorageException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<List<BeerTempAndHumidity>> GetRecentValues(int hoursBack = 24)
        {
            try
            {
                var currentZuluTime = DateTime.Now.ToUniversalTime();
                var startingZuluTime = DateTime.Now.AddHours(-1 * hoursBack).ToUniversalTime();
                var startQueryFilter = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThan, startingZuluTime);
                var endQueryFilter = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, currentZuluTime);
                var combinedFilter = TableQuery.CombineFilters(startQueryFilter, TableOperators.And, endQueryFilter);
                var query = new TableQuery<BeerTempAndHumidity>().Where(combinedFilter);
                var list = (await _beerTable.ExecuteQuerySegmentedAsync(query, null)).Results;
                return list;
            }
            catch(StorageException ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<BeerTempAndHumidity> GetLatest()
        {
            return (await GetRecentValues(1))?.OrderByDescending(x => x.EntryTimestamp).Take(1).First();
        }
    }
}
