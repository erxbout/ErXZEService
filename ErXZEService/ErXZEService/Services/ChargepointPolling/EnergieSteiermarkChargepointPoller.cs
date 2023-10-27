using ErXZEService.Services.ChargepointPolling.Dtos;
using ErXZEService.Services.ChargepointPolling.Interfaces;
using ErXZEService.Services.Configuration;
using ErXZEService.Services.Log;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ErXZEService.Services.ChargepointPolling
{
	public class EnergieSteiermarkChargepointPoller : IEnergieSteiermarkChargepointPoller
	{
		private readonly ILogger _logger;
		private readonly IReadonlyConfiguration _configuration;

		private DateTime LastUpdateTime { get; set; }

		private TimeSpan UpdateCooldown { get; set; }

		public EnergieSteiermarkChargepointPoller(ILogger logger, IReadonlyConfiguration configuration)
        {
			_logger = logger;
			_configuration = configuration;

			UpdateCooldown = TimeSpan.FromSeconds(_configuration.UserSettings.ChargepointIdPolling.UpdateInterval);
		}

        public async Task<ChargepointPollDto> Poll(string chargepointId)
		{
			var result = new ChargepointPollDto()
			{
				ChargepointId = chargepointId
			};

			UpdateCooldown = TimeSpan.FromSeconds(_configuration.UserSettings.ChargepointIdPolling.UpdateInterval);

			if (string.IsNullOrEmpty(chargepointId))
				return result;

			if (DateTime.Now - LastUpdateTime < UpdateCooldown)
				return result;

			try
			{
				LastUpdateTime = DateTime.Now;
				var baseUrl = $"https://lis.e-steiermark.com/sso/api/stations/chargepoint/?evseid={chargepointId}";

				_logger.LogInformation("Start polling EnergieSteiermarkChargepoint item with data: " + chargepointId);

				var url = new Uri(baseUrl);
				var httpClient = new HttpClient();
				var pollResult = await httpClient.GetAsync(url);

				var stringResult = await pollResult.Content.ReadAsStringAsync();

				_logger.LogInformation($"Polling EnergieSteiermarkChargepoint succeeded with result {stringResult}");

				if (!pollResult.IsSuccessStatusCode)
				{
					throw new Exception("Response has no success status code: " + pollResult.StatusCode);
				}

				var objectResult = JsonConvert.DeserializeObject<EnergieSteiermarkChargepointPollDto>(stringResult);

				if (result != null)
				{
					result.Success = objectResult.Success;
					result.Caption = objectResult.Result.Station.Label;
					result.IsAvailable = objectResult.Result.Status == "available";
					result.AvailableStatus = objectResult.Result.Status;
				}

				return result;
			}
			catch (Exception e)
			{
				_logger.LogError("Telemetry item sending error!", e);
				throw;
			}
		}
	}
}
