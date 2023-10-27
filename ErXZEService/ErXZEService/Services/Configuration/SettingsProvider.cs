using ErXZEService.Models.Settings;
using ErXZEService.Services.Log;
using Xamarin.Essentials;

namespace ErXZEService.Services.Configuration
{
    public class SettingsProvider
    {
        public static void Save(SettingsDataItem item)
        {
            Preferences.Set($"{nameof(item.General)}_{nameof(item.General.AutoNavigateToLiveView)}", item.General.AutoNavigateToLiveView);

            Preferences.Set($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.Enabled)}", item.Mqtt.Enabled);
            Preferences.Set($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.HostnameOrIp)}", item.Mqtt.HostnameOrIp);
            Preferences.Set($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.Port)}", item.Mqtt.Port);
            Preferences.Set($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.Username)}", item.Mqtt.Username);
            Preferences.Set($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.Password)}", item.Mqtt.Password);

            Preferences.Set($"{nameof(item.Charger)}_{nameof(item.Charger.Enabled)}", item.Charger.Enabled);
            Preferences.Set($"{nameof(item.Charger)}_{nameof(item.Charger.PossibleChargerAddresses)}", item.Charger.PossibleChargerAddresses);
            Preferences.Set($"{nameof(item.Charger)}_{nameof(item.Charger.EnableAdvancedDiscovery)}", item.Charger.EnableAdvancedDiscovery);
            Preferences.Set($"{nameof(item.Charger)}_{nameof(item.Charger.Type)}", item.Charger.Type);

            Preferences.Set($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.Enabled)}", item.AbrpIntegration.Enabled);
            Preferences.Set($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.UserToken)}", item.AbrpIntegration.UserToken);
            Preferences.Set($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.UpdateInterval)}", item.AbrpIntegration.UpdateInterval);
            Preferences.Set($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.AutoDisableTelemetryChargeCount)}", item.AbrpIntegration.AutoDisableTelemetryChargeCount);
            Preferences.Set($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.SoH)}", item.AbrpIntegration.SoH);

			Preferences.Set($"{nameof(item.ChargepointIdPolling)}_{nameof(item.ChargepointIdPolling.Enabled)}", item.ChargepointIdPolling.Enabled);
			Preferences.Set($"{nameof(item.ChargepointIdPolling)}_{nameof(item.ChargepointIdPolling.UpdateInterval)}", item.ChargepointIdPolling.UpdateInterval);

			Preferences.Set($"{nameof(item.PhotovoltaicIntegration)}_{nameof(item.PhotovoltaicIntegration.Enabled)}", item.PhotovoltaicIntegration.Enabled);
            Preferences.Set($"{nameof(item.PhotovoltaicIntegration)}_{nameof(item.PhotovoltaicIntegration.Type)}", item.PhotovoltaicIntegration.Type);
            Preferences.Set($"{nameof(item.PhotovoltaicIntegration)}_{nameof(item.PhotovoltaicIntegration.FreqencyTopic)}", item.PhotovoltaicIntegration.FreqencyTopic);
        }

        public static SettingsDataItem Restore(ILogger logger)
        {
            var item = new SettingsDataItem(logger);

            item.General.AutoNavigateToLiveView = Preferences.Get($"{nameof(item.General)}_{nameof(item.General.AutoNavigateToLiveView)}", item.General.AutoNavigateToLiveView);

            item.Mqtt.Enabled = Preferences.Get($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.Enabled)}", item.Mqtt.Enabled);
            item.Mqtt.HostnameOrIp = Preferences.Get($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.HostnameOrIp)}", item.Mqtt.HostnameOrIp);
            item.Mqtt.Port = Preferences.Get($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.Port)}", item.Mqtt.Port > 0 ? item.Mqtt.Port : 1883);
            item.Mqtt.Username = Preferences.Get($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.Username)}", item.Mqtt.Username);
            item.Mqtt.Password = Preferences.Get($"{nameof(item.Mqtt)}_{nameof(item.Mqtt.Password)}", item.Mqtt.Password);

            item.Charger.Enabled = Preferences.Get($"{nameof(item.Charger)}_{nameof(item.Charger.Enabled)}", item.Charger.Enabled);
            item.Charger.PossibleChargerAddresses = Preferences.Get($"{nameof(item.Charger)}_{nameof(item.Charger.PossibleChargerAddresses)}", item.Charger.PossibleChargerAddresses);
            item.Charger.EnableAdvancedDiscovery = Preferences.Get($"{nameof(item.Charger)}_{nameof(item.Charger.EnableAdvancedDiscovery)}", item.Charger.EnableAdvancedDiscovery);
            item.Charger.Type = Preferences.Get($"{nameof(item.Charger)}_{nameof(item.Charger.Type)}", item.Charger.Type);

            item.AbrpIntegration.Enabled = Preferences.Get($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.Enabled)}", item.AbrpIntegration.Enabled);
            item.AbrpIntegration.UserToken = Preferences.Get($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.UserToken)}", item.AbrpIntegration.UserToken);
            item.AbrpIntegration.UpdateInterval = Preferences.Get($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.UpdateInterval)}", item.AbrpIntegration.UpdateInterval);
            item.AbrpIntegration.AutoDisableTelemetryChargeCount = Preferences.Get($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.AutoDisableTelemetryChargeCount)}", item.AbrpIntegration.AutoDisableTelemetryChargeCount);
            item.AbrpIntegration.SoH = Preferences.Get($"{nameof(item.AbrpIntegration)}_{nameof(item.AbrpIntegration.SoH)}", item.AbrpIntegration.SoH);

			item.ChargepointIdPolling.Enabled = Preferences.Get($"{nameof(item.ChargepointIdPolling)}_{nameof(item.ChargepointIdPolling.Enabled)}", item.ChargepointIdPolling.Enabled);
			item.ChargepointIdPolling.UpdateInterval = Preferences.Get($"{nameof(item.ChargepointIdPolling)}_{nameof(item.ChargepointIdPolling.UpdateInterval)}", item.ChargepointIdPolling.UpdateInterval);

			item.PhotovoltaicIntegration.Enabled = Preferences.Get($"{nameof(item.PhotovoltaicIntegration)}_{nameof(item.PhotovoltaicIntegration.Enabled)}", item.PhotovoltaicIntegration.Enabled);
            item.PhotovoltaicIntegration.Type = Preferences.Get($"{nameof(item.PhotovoltaicIntegration)}_{nameof(item.PhotovoltaicIntegration.Type)}", item.PhotovoltaicIntegration.Type);
            item.PhotovoltaicIntegration.FreqencyTopic = Preferences.Get($"{nameof(item.PhotovoltaicIntegration)}_{nameof(item.PhotovoltaicIntegration.FreqencyTopic)}", item.PhotovoltaicIntegration.FreqencyTopic);

            return item;
        }
    }
}
