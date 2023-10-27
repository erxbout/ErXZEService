using ErXZEService.Services.Charger;
using ErXZEService.Services.Charger.GoeCharger;
using ErXZEService.Services.Log;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErXZEService.Models.Settings
{
    public class ChargerSettings
    {
        private ILogger _logger;

        public Action OnChangeSettings { get; set; }

        public bool Enabled { get; set; }

        public List<string> Types { get; set; } = new List<string>() { "None", "goe-charger" };

        public string Type { get; set; } = "None";

        public string PossibleChargerAddresses { get; set; }

        public bool EnableAdvancedDiscovery { get; set; }

        public StateViewItem TestState { get; set; }

        public ChargerSettings(ILogger logger)
        {
            _logger = logger;
        }

        public void TestSettings()
        {
            Task.Run(() =>
            {
                ICharger foundCharger = null;

                if (!string.IsNullOrEmpty(PossibleChargerAddresses))
                {
                    var endpoints = PossibleChargerAddresses.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    foreach (var endpoint in endpoints)
                    {
                        var charger = new GoeCharger(new GoeChargerSettingItem() { Endpoint = endpoint }, _logger);

                        if (charger.RefreshState())
                        {
                            foundCharger = charger;
                            break;
                        }
                    }
                }

                var testStateEnum = foundCharger != null 
                    ? Settings.TestState.Success 
                    : Settings.TestState.Fail;

                TestState = SettingsDataItem.GetState(testStateEnum);

                OnChangeSettings();
            });
        }
    }
}
