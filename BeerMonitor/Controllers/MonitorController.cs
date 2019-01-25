using System.Linq;
using System.Threading.Tasks;
using BeerMonitor.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BeerMonitor.Controllers
{
    [ApiController]
    public class MonitorController : ControllerBase
    {
        private readonly ITemperatureLogicHandler _temperatureLogicHandler;
        private readonly IBlobStorageService _blobStorageService;
        private readonly INestService _nestService;

        public MonitorController(ITemperatureLogicHandler temperatureLogichandler, IBlobStorageService blobStorageService, INestService nestService)
        {
            _temperatureLogicHandler = temperatureLogichandler;
            _blobStorageService = blobStorageService;
            _nestService = nestService;
        }

        // GET
        [HttpGet]
        [Route("api/monitor")]
        public async Task<IActionResult> GetRecentValues()
        {
            var results = await _blobStorageService.GetRecentValues();
            if (results != null && results.Any())
            {
                return new OkObjectResult(results);
            }
            return new NotFoundResult();
        }

        [HttpGet]
        [Route("api/monitor/{hours}")]
        public async Task<IActionResult> GetRecentValues(int hours)
        {
            var results = await _blobStorageService.GetRecentValues(hours);
            if (results != null && results.Any())
            {
                return new OkObjectResult(results);
            }
            return new NotFoundResult();
        }

        [HttpGet]
        [Route("api/monitor/latest")]
        public async Task<IActionResult> GetLatest()
        {
            var result = await _blobStorageService.GetLatest();
            if(result != null)
            {
                return new OkObjectResult(result);
            }
            return new NotFoundResult();
        }

        // POST api/values
        [HttpPost]
        [Route("api/monitor")]
        public async Task<IActionResult> PostUpdate(string temp, string humidity)
        {
            if(await _temperatureLogicHandler.UpdateTemperature(temp, humidity))
            {
                return new OkResult();
            }
            return new StatusCodeResult(500);
        }
    }
}
