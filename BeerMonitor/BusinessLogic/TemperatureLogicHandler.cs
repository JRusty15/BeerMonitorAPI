using BeerMonitor.Interfaces;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeerMonitor.BusinessLogic
{
    public class TemperatureLogicHandler : ITemperatureLogicHandler
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly INestService _nestService;
        private TelemetryClient telemetry = new TelemetryClient();

        public TemperatureLogicHandler(IBlobStorageService blobStorageService, INestService nestService)
        {
            _blobStorageService = blobStorageService;
            _nestService = nestService;
        }

        public async Task<bool> UpdateTemperature(string temp, string humidity)
        {
            double dTemperature = -1, dHumidity = -1;
            if (!double.TryParse(temp, out dTemperature) ||
               !double.TryParse(humidity, out dHumidity))
            {
                return false;
            }

            if (dTemperature < 60 || dTemperature > 70)
            {
                telemetry.TrackEvent("Temp out of bounds",
                    new Dictionary<string, string> { { "Temp", temp.ToString() }, { "Humidity", humidity.ToString() } },
                    null);

                var ambiantTemp = await _nestService.GetCurrentAmbiantTemperature();
                var targetTemp = await _nestService.GetCurrentTargetTemperature();

                if (ambiantTemp.HasValue && targetTemp.HasValue)
                {
                    if (dTemperature < 60)
                    {
                        if (targetTemp.Value <= ambiantTemp.Value)
                        {
                            await _nestService.SetTemperature(ambiantTemp.Value + 1);
                        }
                        else
                        {
                            telemetry.TrackTrace($"Beer temperature was {dTemperature} but did not update thermostat because ambiant temp is {ambiantTemp.Value} and target temp is {targetTemp.Value}");
                        }
                    }
                    else
                    {
                        if (targetTemp.Value >= ambiantTemp.Value)
                        {
                            await _nestService.SetTemperature(ambiantTemp.Value - 1);
                        }
                        else
                        {
                            telemetry.TrackTrace($"Beer temperature was {dTemperature} but did not update thermostat because ambiant temp is {ambiantTemp.Value} and target temp is {targetTemp.Value}");
                        }
                    }
                }
            }

            if (await _blobStorageService.InsertTempAndHumidity(dTemperature, dHumidity))
            {
                return true;
            }
            
            return false;
        }
    }
}
