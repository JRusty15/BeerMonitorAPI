using Microsoft.Extensions.Configuration;

namespace BeerMonitor.BusinessLogic
{
    public static class ConfigurationManager
    {
#if DEBUG
        private static readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();
#else
        private static readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
#endif

        public static string NestAuthToken => _config["NestAuthToken"];
        public static string NestThermostatId => _config["NestThermostatId"];
        public static string BlobName =>_config["BlobName"];
        public static string BlobKey => _config["BlobKey"];
        public static string BlobTableName => _config["BlobTableName"];
        public static int MinTemperature => int.Parse(_config["MinTemperature"]);
        public static int MaxTemperature => int.Parse(_config["MaxTemperature"]);
    }
}
