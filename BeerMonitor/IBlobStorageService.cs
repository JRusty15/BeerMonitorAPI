using System.Threading.Tasks;

namespace BeerMonitor
{
    public interface IBlobStorageService
    {
        Task<bool> InsertTempAndHumidity(double temp, double humidity);
    }
}
