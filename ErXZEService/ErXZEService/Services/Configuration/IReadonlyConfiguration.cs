using ErXZEService.Models.Settings;

namespace ErXZEService.Services.Configuration
{
    public interface IReadonlyConfiguration
    {
        SettingsDataItem UserSettings { get; }

        void Reload();
    }
}