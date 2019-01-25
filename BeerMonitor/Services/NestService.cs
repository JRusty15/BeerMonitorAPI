using BeerMonitor.BusinessLogic;
using BeerMonitor.Interfaces;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BeerMonitor.Services
{
    public class NestService : INestService
    {
        private TelemetryClient telemetry = new TelemetryClient();
        private readonly HttpClient _httpClient;

        public NestService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.NestAuthToken);
        }

        private async Task<int?> GetCurrentTemperature(string temperatureLabel)
        {
            var requestUri = $"https://developer-api.nest.com/devices/thermostats/{ConfigurationManager.NestThermostatId}/{temperatureLabel}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await SendRequestToNest(requestMessage);

            if(response != null)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                int temp = -1;
                if (int.TryParse(responseData, out temp))
                {
                    return temp;
                }
            }

            return null;
        }

        /// <summary>
        /// This handles the redirect and making sure to send the auth header to that redirect
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendRequestToNest(HttpRequestMessage requestMessage)
        {
            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var finalRequestUri = response.RequestMessage.RequestUri;
                    var newMessage = new HttpRequestMessage(requestMessage.Method, finalRequestUri);
                    newMessage.Content = requestMessage.Content;
                    response = await _httpClient.SendAsync(newMessage);
                }

                return response;
            }
            catch (Exception ex)
            {
                telemetry.TrackTrace($"Failed to send request to Nest: {ex.Message}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error, 
                        new Dictionary<string, string> {{"Request", requestMessage.RequestUri.ToString()},{"Method", requestMessage.Method.ToString()}});
                telemetry.TrackException(ex);
            }

            return null;
        }

        public async Task<int?> GetCurrentAmbiantTemperature()
        {
            return await GetCurrentTemperature("ambient_temperature_f");
        }

        public async Task<int?> GetCurrentTargetTemperature()
        {
            return await GetCurrentTemperature("target_temperature_f");
        }

        public async Task<bool> SetTemperature(int tempValue)
        {
            var messageBody = $"{{\"target_temperature_f\": {tempValue}}}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"https://developer-api.nest.com/devices/thermostats/{ConfigurationManager.NestThermostatId}");
            requestMessage.Content = new StringContent(messageBody, System.Text.Encoding.UTF8, "application/json");

#if DEBUG
            Console.WriteLine($"Debug mode: request to set temperature to {tempValue}");
            return true;
#else
            var response = await SendRequestToNest(requestMessage);
            if(response.IsSuccessStatusCode)
            {
                telemetry.TrackTrace($"Successfully set temperature to {tempValue}");
                return true;
            }
           
            telemetry.TrackTrace($"Failed to set temperature to {tempValue} -- {response.ReasonPhrase}");
            return false;
#endif
        }
    }
}
