using ErXZEService.Services.Configuration;
using ErXZEService.Services.Log;
using System;
using System.Collections.Generic;

namespace ErXZEService.Models.Settings
{
    public enum TestState : byte
    {
        Testing,
        Success,
        TooFastTesting,
        Fail
    }

    public class SettingsDataItem
    {
        public static List<StateViewItem> PossibleTestStates = new List<StateViewItem>()
        {
            new StateViewItem()
            {
                ImageSource = "Attention.png",
                State = "Testing.."
            },

            new StateViewItem()
            {
                ImageSource = "ok.png",
                State = "Successful"
            },
            
            new StateViewItem()
            {
                ImageSource = "Attention.png",
                State = "Too fast Testing!"
            },

            new StateViewItem()
            {
                ImageSource = "fail.png",
                State = "Failed"
            }
        };
        private ILogger _logger;

        public SettingsDataItem(ILogger logger)
        {
            _logger = logger;

            Charger = new ChargerSettings(_logger);
        }

        public Action OnChangeSettings
        {
            set
            {
                Charger.OnChangeSettings = value;
                Mqtt.OnChangeSettings = value;
                AbrpIntegration.OnChangeSettings = value;
                ChargepointIdPolling.OnChangeSettings = value;
            }
        }

        public GeneralSettings General { get; set; } = new GeneralSettings();

        public MqttSettings Mqtt { get; set; } = new MqttSettings();

        public ChargerSettings Charger { get; set; }

        public PhotovoltaicIntegrationSettings PhotovoltaicIntegration { get; set; } = new PhotovoltaicIntegrationSettings();
        public AbrpSettings AbrpIntegration { get; set; } = new AbrpSettings();
        public ChargepointIdPollingSettings ChargepointIdPolling { get; set; } = new ChargepointIdPollingSettings();

        public static StateViewItem GetState(TestState state)
        {
            return PossibleTestStates[(byte)state];
        }

        internal void Save()
        {
            SettingsProvider.Save(this);
        }
    }
}
