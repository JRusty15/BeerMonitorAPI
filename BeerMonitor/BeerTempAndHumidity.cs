using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace BeerMonitor
{
    public class BeerTempAndHumidity : TableEntity
    {
        public BeerTempAndHumidity()
        {
        }

        public BeerTempAndHumidity(double temperature, double humidity, DateTime timestamp)
        {
            Temperature = temperature;
            Humidity = humidity;
            EntryTimestamp = timestamp;
            PartitionKey = timestamp.ToString("MMddyyyy");
            RowKey = Guid.NewGuid().ToString();
        }

        public double Temperature { get; set;  }
        public double Humidity { get; set; }
        public DateTime EntryTimestamp { get; set; }
    }
}
