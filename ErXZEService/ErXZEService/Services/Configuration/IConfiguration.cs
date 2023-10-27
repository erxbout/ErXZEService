using ErXZEService.Models.Settings;

namespace ErXZEService.Services.Configuration
{
    public interface IConfiguration
    {
        SettingsDataItem UserSettings { get; set; }

        void Reload();
        void Save();
    }
}