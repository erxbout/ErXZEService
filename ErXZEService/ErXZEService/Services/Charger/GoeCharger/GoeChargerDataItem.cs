using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ErXZEService.Services.Charger.GoeCharger
{
    public enum StopState : byte
    {
        Disabled,
        ReachingKwhThreshold = 2
    }

    public enum ChargerState : byte
    {
        ReadyNoCar = 1,
        CarCharging,
        WaitingForCar,
        ChargingFinished
    }

    public class GoeChargerDataItem : IChargerDataItem
    {
        [JsonProperty("amp")]
        public int PilotAmpere { get; set; }

        [JsonProperty("cbl")]
        public int MaxCableAmpere { get; set; }

        public int MaxDesiredCurrent => MaxCableAmpere > 0 ? Math.Min(MaxCableAmpere, AdapterCableAmpere) : AdapterCableAmpere;

        [JsonProperty("adi")]
        public int Adapter { get; set; }

        public bool IsAdapterInUse => Adapter == 1;

        private int AdapterCableAmpere => IsAdapterInUse ? 16 : 32;

        [JsonProperty("ama")]
        public int AbsoluteMaxAmpere { get; set; }

        [JsonProperty("alw")]
        public int AllowCharging { get; set; }

        public bool AllowChargingBool => AllowCharging != 0;

        [JsonProperty("stp")]
        public StopState StopState { get; set; }

        [JsonProperty("car")]
        public ChargerState ChargerState { get; set; }

        [JsonProperty("dws")]
        public int CurrentChargeWs { get; set; }

        public decimal CurrentChargeKwh => Math.Round((decimal)CurrentChargeWs / 360000, 2);

        [JsonProperty("dwo")]
        public int KwhThresholdInt { get; set; }

        public decimal KwhThreshold => (decimal)KwhThresholdInt / 10;

        [JsonProperty("eto")]
        public int LivetimeCharged { get; set; }

        public decimal LivetimeChargedKwh => (decimal)LivetimeCharged / 10;

        [JsonProperty("tmp")]
        public int ChargerTemperature { get; set; }

        [JsonProperty("nrg")]
        public List<int> EnergyStatus { get; set; } = new List<int>(16);

        public int VoltageL1 => EnergyStatusAvailable ? EnergyStatus[0] > 4 ? EnergyStatus[0] : EnergyStatus[3] : 0;
        public int VoltageL2 => EnergyStatusAvailable ? EnergyStatus[1] : 0;
        public int VoltageL3 => EnergyStatusAvailable ? EnergyStatus[2] : 0;
        public int VoltageN => EnergyStatusAvailable ? EnergyStatus[3] > 4 ? EnergyStatus[0] : EnergyStatus[3] : 0;

        public decimal Voltage => EnergyStatusAvailable ? 
            Math.Round(EnergyStatus.Take(Phases).Average(x => (decimal)x) * AverageFactor, 1) < 4 ? VoltageL1 : 
            Math.Round(EnergyStatus.Take(Phases).Average(x => (decimal)x) * AverageFactor, 1) : 0;

        public int Phases
        {
            get
            {
                int maxPhases = 3;
                for (int i = 0; i < maxPhases; i++)
                {
                    if (EnergyStatusAvailable && EnergyStatus[i] == 0)
                        return i;
                }

                return maxPhases;
            }
        }

        public decimal AmpereL1 => EnergyStatusAvailable ? EnergyStatus[4]  / 10m : 0;
        public decimal AmpereL2 => EnergyStatusAvailable ? EnergyStatus[5] / 10m : 0;
        public decimal AmpereL3 => EnergyStatusAvailable ? EnergyStatus[6] / 10m : 0;

        public decimal PowerL1 => EnergyStatusAvailable ? EnergyStatus[7]  / 10m : 0;
        public decimal PowerL2 => EnergyStatusAvailable ? EnergyStatus[8]  / 10m : 0;
        public decimal PowerL3 => EnergyStatusAvailable ? EnergyStatus[9] / 10m : 0;
        public decimal PowerN => EnergyStatusAvailable ? EnergyStatus[10] / 10m : 0;

        public decimal Power => EnergyStatusAvailable ? EnergyStatus[11] / 100m : 0;

        public decimal PowerFactorL1 => EnergyStatusAvailable ? EnergyStatus[12]  / 100m : 0;
        public decimal PowerFactorL2 => EnergyStatusAvailable ? EnergyStatus[13]  / 100m : 0;
        public decimal PowerFactorL3 => EnergyStatusAvailable ? EnergyStatus[14] / 100m : 0;

        public decimal PowerFactorN => EnergyStatusAvailable ? EnergyStatus[15] / 10m : 0;

        /// <summary>
        /// root of 3 if Phases are 3
        /// </summary>
        private decimal AverageFactor => Phases == 3 ? (decimal)Math.Sqrt(3) : 1;

        private bool EnergyStatusAvailable => EnergyStatus?.Count == 16;
    }
}
