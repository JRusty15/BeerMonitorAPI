using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace BeerMonitor
{
    public class BeerTempAndHumidity : TableEntity
    {
        public BeerTempAndHumidity(double temperature, double humidity, DateTime timestamp)
        {
            Temperature = temperature;
            Humidity = humidity;
            EntryTimestamp = timestamp.ToString();
            PartitionKey = timestamp.ToString("MMyyyy");
            RowKey = Guid.NewGuid().ToString();
        }

        public double Temperature { get; set;  }
        public double Humidity { get; set; }
        public string EntryTimestamp { get; set; }
    }
}
