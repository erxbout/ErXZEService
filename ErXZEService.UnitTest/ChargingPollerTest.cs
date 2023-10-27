using DryIoc;
using ErXZEService.Models.Settings;
using ErXZEService.Services;
using ErXZEService.Services.ChargepointPolling;
using ErXZEService.Services.ChargepointPolling.Interfaces;
using ErXZEService.Services.Configuration;
using ErXZEService.Services.Events;
using ErXZEService.Services.Log;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace ErXZEService.UnitTest
{
	public class FakeConfiguration : IConfiguration, IReadonlyConfiguration
	{
		private readonly ILogger _logger;

		public SettingsDataItem UserSettings { get; set; }

		public FakeConfiguration(ILogger logger)
		{
			_logger = logger;

			UserSettings = new SettingsDataItem(_logger)
			{
				ChargepointIdPolling = new ChargepointIdPollingSettings()
				{
					UpdateInterval = 60
				}
			};
		}

		public void Reload()
		{
			
		}

		public void Save()
		{
		}
	}

	[TestClass]
    public class ChargingPollerTest
	{
        [TestMethod]
        public async Task TestChargingPoller_EnergieSteiermark()
        {
			IoC.IoContainer = new Container();
			IoC.IoContainer.Register<ILogger, FakeLogger>(new SingletonReuse());
			IoC.IoContainer.Register<IReadonlyConfiguration, FakeConfiguration>(new SingletonReuse());
			IoC.IoContainer.Register<IEnergieSteiermarkChargepointPoller, EnergieSteiermarkChargepointPoller>(new SingletonReuse());

			var service = IoC.Resolve<IEnergieSteiermarkChargepointPoller>();

            var result = await service.Poll("AT*EST*E000*00001");

			result.Success.Should().Be(true);
			result.Caption.Should().Be("E-Mobil Shop Graz");
			result.ChargepointId.Should().Be("AT*EST*E000*00001");
        }
    }
}
