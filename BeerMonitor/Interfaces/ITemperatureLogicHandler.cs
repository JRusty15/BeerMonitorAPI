using System.Threading.Tasks;

namespace BeerMonitor.Interfaces
{
    public interface ITemperatureLogicHandler
    {
        Task<bool> UpdateTemperature(string temp, string humidity);
    }
}