using System.Threading.Tasks;

namespace BeerMonitor.Interfaces
{
    public interface INestService
    {
        Task<int?> GetCurrentAmbiantTemperature();
        Task<int?> GetCurrentTargetTemperature();
        Task<bool> SetTemperature(int tempValue);
    }
}
