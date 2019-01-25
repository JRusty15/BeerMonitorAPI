using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeerMonitor.Interfaces
{
    public interface IBlobStorageService
    {
        Task<bool> InsertTempAndHumidity(double temp, double humidity);
        Task<List<BeerTempAndHumidity>> GetRecentValues(int hoursBack = 24);
        Task<BeerTempAndHumidity> GetLatest();
    }
}
