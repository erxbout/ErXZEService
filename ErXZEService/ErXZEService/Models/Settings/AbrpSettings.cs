using ErXZEService.Services;
using ErXZEService.Services.Abrp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErXZEService.Models.Settings
{
    public class UpdateInterval
    {
        public int IntervalInSeconds { get; set; }

        public string Caption { get; set; }

        public override string ToString()
        {
            return Caption;
        }
    }

    public class AbrpSettings
    {
        public Action OnChangeSettings { get; set; }

        public bool Enabled { get; set; }

        /// <summary>
        /// token for identifying where live data should be shown
        /// </summary>
        public string UserToken { get; set; }

        public List<UpdateInterval> UpdateIntervalTypes { get; set; } = new List<UpdateInterval>()
        {
            new UpdateInterval() { IntervalInSeconds = 3, Caption = "fastest (every 3s)" },
            new UpdateInterval() { IntervalInSeconds = 5, Caption = "fast (every 5s)" },
            new UpdateInterval() { IntervalInSeconds = 10, Caption = "recommended (every 10s)" },
            new UpdateInterval() { IntervalInSeconds = 30, Caption = "slow (every 30s)" },
            new UpdateInterval() { IntervalInSeconds = 60, Caption = "slowest (every 1m)" }
        };

        public UpdateInterval UpdateIntervalType { get; set; }

        public int UpdateInterval
        {
            get
            {
                return UpdateIntervalType.IntervalInSeconds;
            }
            set
            {
                UpdateIntervalType = UpdateIntervalTypes.FirstOrDefault(x => x.IntervalInSeconds == value);
            }
        }

        public int AutoDisableTelemetryChargeCount { get; set; }

        public int SoH { get; set; }

        public StateViewItem TestState { get; set; }

        public AbrpSettings()
        {
            UpdateIntervalType = UpdateIntervalTypes.First(x => x.IntervalInSeconds == 10);
        }

        public void TestSettings()
        {
            Task.Run(async () =>
            {
                var service = IoC.Resolve<IAbrpTelemetryService>();

                service.UserToken = UserToken;

                var currentData = GlobalDataStore.DataItemManager.CurrentDataItem;
                bool? success = null;

                try
                {
                    success = await service.SendState(new ElectricCarTelemetryItem()
                    {
                        AvailableEnergy = currentData.AvaliableEnergy,
                        BatteryTemperature = currentData.BatteryTemperature,
                        EstimatedRange = currentData.EstimatedRange,
                        ExternalTemperature = currentData.AmbientTemperature.GetValueOrDefault(0),
                        IsCharging = currentData.State == ElectricCarState.Charging,
                        IsParked = currentData.GearLeverPosition == GearLeverPosition.Park,
                        Power = currentData.State == ElectricCarState.Charging ? currentData.ChargingPower * -1 : currentData.Consumption.GetValueOrDefault(0),
                        Soc = currentData.SoC,
                        Speed = currentData.Speed.GetValueOrDefault(0)
                    });
                }
                catch(Exception e)
                {
                }

                var testStateEnum = 
                    success == null 
                        ? Settings.TestState.Fail :
                        success.Value
                            ? Settings.TestState.Success
                            : Settings.TestState.TooFastTesting;

                TestState = SettingsDataItem.GetState(testStateEnum);

                OnChangeSettings();
            });
        }
    }
}
