using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BeerMonitor.Controllers
{
    [ApiController]
    public class MonitorController : ControllerBase
    {
        private BlobStorageService blobStorageService = new BlobStorageService();

        // GET
        [HttpGet]
        [Route("api/monitor")]
        public async Task<IActionResult> GetRecentValues()
        {
            return new OkObjectResult("Recent");
        }

        [HttpGet]
        [Route("api/monitor/latest")]
        public async Task<IActionResult> GetLatest()
        {
            return new OkObjectResult("Latest");
        }

        // POST api/values
        [HttpPost]
        [Route("api/monitor")]
        public async Task<IActionResult> PostUpdate(string temp, string humidity)
        {
            double dTemperature = -1, dHumidity = -1;
            if (double.TryParse(temp, out dTemperature) &&
               double.TryParse(humidity, out dHumidity))
            {
                if (await blobStorageService.InsertTempAndHumidity(dTemperature, dHumidity))
                {
                    return new OkResult();
                }
            }
            return new StatusCodeResult(500);
        }
    }
}
