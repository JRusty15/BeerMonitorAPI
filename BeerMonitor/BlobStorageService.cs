using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace BeerMonitor
{
    public class BlobStorageService : IBlobStorageService
    {
        private static string _blobName = "beerblobstorage";
        private static string _blobKey = "TugmWwpwRm3iAPfRJwR55GTzEQYH5UijxhHaSefpEW1NSvoJMxDlpja/zXnPtf8VhEMYkR1ZcXBHyMe+lY1CYg==";
        private CloudTable _beerTable;

        public BlobStorageService()
        {
            StorageCredentials creds = new StorageCredentials(_blobName, _blobKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(creds, true);
            var client = storageAccount.CreateCloudTableClient();
            _beerTable = client.GetTableReference("beertempdata");
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

        public async Task<List<BeerTempAndHumidity>> GetRecentValues()
        {
            try
            {
                var todayDateKey = DateTime.Today.ToString("MMddyyyy");
                var yesterdayDateKey = DateTime.Today.AddDays(-1).ToString("MMddyyyy");
                var todayQueryFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, todayDateKey);
                var yesterdayQueryFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, yesterdayDateKey);
                var combinedFilter = TableQuery.CombineFilters(todayQueryFilter, TableOperators.Or, yesterdayQueryFilter);
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
            return (await GetRecentValues())?.OrderByDescending(x => x.EntryTimestamp).Take(1).First();
        }
    }
}
