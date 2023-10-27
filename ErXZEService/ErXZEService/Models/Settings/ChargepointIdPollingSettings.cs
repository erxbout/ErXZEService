using ErXZEService.Services;
using ErXZEService.Services.ChargepointPolling.Dtos;
using ErXZEService.Services.ChargepointPolling.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErXZEService.Models.Settings
{
	public class ChargepointIdPollingSettings
	{
        public Action OnChangeSettings { get; set; }

        public bool Enabled { get; set; }

        public List<UpdateInterval> UpdateIntervalTypes { get; set; } = new List<UpdateInterval>()
        {
            new UpdateInterval() { IntervalInSeconds = 60, Caption = "fastest (every 1min)" },
            new UpdateInterval() { IntervalInSeconds = 120, Caption = "fast (every 2min)" },
            new UpdateInterval() { IntervalInSeconds = 300, Caption = "recommended (every 5min)" },
            new UpdateInterval() { IntervalInSeconds = 600, Caption = "slow (every 10min)" },
            new UpdateInterval() { IntervalInSeconds = 900, Caption = "slowest (every 15m)" }
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

        public StateViewItem TestState { get; set; }

        public ChargepointIdPollingSettings()
        {
            UpdateIntervalType = UpdateIntervalTypes.First(x => x.IntervalInSeconds == 300);
        }

        public void TestSettings()
        {
            Task.Run(async () =>
            {
                var service = IoC.Resolve<IEnergieSteiermarkChargepointPoller>();

				ChargepointPollDto success = null;

                try
                {
                    success = await service.Poll("AT*EST*E000*00001");
                }
                catch(Exception e)
                {
                }

                var testStateEnum = 
                    success == null 
                        ? Settings.TestState.Fail :
                        success != null && success.Success
                            ? Settings.TestState.Success
                            : Settings.TestState.TooFastTesting;

                TestState = SettingsDataItem.GetState(testStateEnum);

                OnChangeSettings();
            });
        }
    }
}
