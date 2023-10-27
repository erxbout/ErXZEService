using ErXZEService.Models.Settings;
using ErXZEService.Services.Log;

namespace ErXZEService.Services.Configuration
{
    public class Configuration : IConfiguration, IReadonlyConfiguration
    {
        private readonly ILogger _logger;

        public SettingsDataItem UserSettings { get; set; }

        public Configuration(ILogger logger)
        {
            _logger = logger;

            Reload();
        }

        public void Reload()
        {
            UserSettings = SettingsProvider.Restore(_logger);
        }

        public void Save()
        {
            SettingsProvider.Save(UserSettings);
            IoC.Resolve<IReadonlyConfiguration>().Reload();
        }
    }
}
