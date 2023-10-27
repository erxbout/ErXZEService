using ErXZEService.Services.Configuration;
using ErXZEService.Services.Log;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ErXZEService.Services.Abrp
{
    public class AbrpTelemetryService : IAbrpTelemetryService
    {
        private ILogger _logger;
        private readonly IReadonlyConfiguration _configuration;

        public string UserToken { private get; set; }

        private DateTime LastUpdateTime { get; set; }

        private TimeSpan UpdateCooldown { get; set; }

        public AbrpTelemetryService(ILogger logger, IReadonlyConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            UserToken = _configuration.UserSettings.AbrpIntegration.UserToken;
            UpdateCooldown = TimeSpan.FromSeconds(_configuration.UserSettings.AbrpIntegration.UpdateInterval);
        }

        public async Task<bool> SendState(ElectricCarTelemetryItem item)
        {
            UserToken = _configuration.UserSettings.AbrpIntegration.UserToken;
            UpdateCooldown = TimeSpan.FromSeconds(_configuration.UserSettings.AbrpIntegration.UpdateInterval);

            if (item.AvailableEnergy < 0 || item.BatteryTemperature < 0 || item.EstimatedRange < 0)
                return false;

            if (DateTime.Now - LastUpdateTime < UpdateCooldown)
                return false;

            try
            {
                LastUpdateTime = DateTime.Now;
                item.TimeInUnixEpoch = DateTimeOffset.Now.ToUnixTimeSeconds();

                var itemJson = JsonConvert.SerializeObject(item);
                var baseUrl = $"https://api.iternio.com/1/tlm/send?token={UserToken}&tlm={itemJson}";

                _logger.LogInformation("Start sending Telemetry item with data: " + itemJson);

                var url = new Uri(baseUrl);
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", "APIKEY 0000000-0000-0000-0000-00000000000");
                var result = await httpClient.PostAsync(url, new StringContent(""));

                var stringResult = await result.Content.ReadAsStringAsync();

                _logger.LogInformation($"Telemetry item delivered with result {stringResult}");

                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception("Response has no success status code: " + result.StatusCode);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Telemetry item sending error!", e);
                throw;
            }
        }
    }
}
