using ErXBoutCode.MVVM;
using ErXZEService.Models.Settings;
using ErXZEService.Services;
using ErXZEService.Services.Configuration;
using ErXZEService.Services.Log;
using Xamarin.Forms;

namespace ErXZEService.ViewModels.Settings
{
    public class SettingsViewModel : BaseViewModel
    {
        private IConfiguration _configuration;
        private IReadonlyConfiguration _readonlyConfiguration;

        public SettingsViewModel(ContentPage parent)
        {
            ParentContentPage = parent;

            Save = new ActionCommand(OnSave);
            TestMqtt = new ActionCommand(OnTestMqtt);
            TestCharger = new ActionCommand(OnTestCharger);
            TestAbrpIntegration = new ActionCommand(OnTestAbrpIntegration);
			TestChargepointIdPolling = new ActionCommand(OnTestChargepointIdPolling);

            Settings = SettingsProvider.Restore(IoC.Resolve<ILogger>());
            _configuration = IoC.Resolve<IConfiguration>();
            _readonlyConfiguration = IoC.Resolve<IReadonlyConfiguration>();

            Settings.Mqtt.TestState = SettingsDataItem.GetState(TestState.Success);
            Settings.Charger.TestState = SettingsDataItem.GetState(TestState.Success);

            Settings.OnChangeSettings = OnChangeSettings;
        }
        
        public ContentPage ParentContentPage { get; private set; }
        
        public ActionCommand Save { get; set; }

        public ActionCommand TestMqtt { get; set; }

        public ActionCommand TestCharger { get; set; }

        public ActionCommand TestAbrpIntegration { get; set; }
        public ActionCommand TestChargepointIdPolling { get; set; }

        public SettingsDataItem Settings { get; set; } = new SettingsDataItem(IoC.Resolve<ILogger>());
        
        private async void OnSave()
        {
            var result = await ParentContentPage.DisplayAlert("Save?", "Really want to save", "Yes", "Cancel");

            if (result)
            {
                Settings.Save();
                _configuration.Reload();
                _readonlyConfiguration.Reload();
            }
        }

        private void OnTestMqtt()
        {
            Settings.Mqtt.TestState = SettingsDataItem.GetState(TestState.Testing);
            PropChanged(nameof(Settings));

            Settings.Mqtt.TestSettings();
        }

        private void OnTestCharger()
        {
            Settings.Charger.TestState = SettingsDataItem.GetState(TestState.Testing);
            PropChanged(nameof(Settings));
            
            Settings.Charger.TestSettings();
        }

        private void OnTestAbrpIntegration()
        {
            Settings.AbrpIntegration.TestState = SettingsDataItem.GetState(TestState.Testing);
            PropChanged(nameof(Settings));

            Settings.AbrpIntegration.TestSettings();
        }

        private void OnTestChargepointIdPolling()
        {
            Settings.ChargepointIdPolling.TestState = SettingsDataItem.GetState(TestState.Testing);
            PropChanged(nameof(Settings));

            Settings.ChargepointIdPolling.TestSettings();
        }

        private void OnChangeSettings()
        {
            PropChanged(nameof(Settings));
        }
    }
}
