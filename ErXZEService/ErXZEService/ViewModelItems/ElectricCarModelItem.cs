using ErXBoutCode.MVVM.Property;
using ErXZEService.Services;
using System;
using System.Linq;

namespace ErXZEService.Models
{
    public class ElectricCarModelItem
    {
        public ElectricCarDataItem DataItem
        {
            get
            {
                return GlobalDataStore.DataItemManager?.CurrentDataItem;
            }
        }

        public string State
        {
            get { return DataItem.State.ToString(); }
        }

        public string StateImageSource
        {
            get
            {
                switch (DataItem.State)
                {
                    case ElectricCarState.Parked:
                        return "carparkbreak.png";
                    case ElectricCarState.Driving:
                        return "BlueThunder.png";
                    case ElectricCarState.Charging:
                        return "BatteryChargingSymbol.png";
                    default:
                        break;
                }

                return "carparkbreak.png";
            }
        }

        public string FrontLeftPressure => Math.Round(DataItem.FrontLeftPressure.GetValueOrDefault(0) * 130.725m, 0) + "mB";
        public string BackLeftPressure => Math.Round(DataItem.BackLeftPressure.GetValueOrDefault(0) * 130.725m, 0) + "mB";

        public string FrontRightPressure => Math.Round(DataItem.FrontRightPressure.GetValueOrDefault(0) * 130.725m, 0) + "mB";
        public string BackRightPressure => Math.Round(DataItem.BackRightPressure.GetValueOrDefault(0) * 130.725m, 0) + "mB";

        public bool IsCruiseControlEnabled => DataItem.CCSpeed != byte.MaxValue && DataItem.CCSpeed > 0 && DataItem.CCMode == 4;

        public string CruiseControlImageSource => IsCruiseControlEnabled ? "cruisecontrol.png" : "cruisecontrol_off.png";

        public CombinedString<byte> CruiseControlSpeed
        {
            get { return new CombinedString<byte>(DataItem.CCSpeed.GetValueOrDefault(0)) { StringValue = DataItem.CCSpeed.GetValueOrDefault(0) < (byte.MaxValue - 1) ? DataItem.CCSpeed.GetValueOrDefault(0).ToString() + "km/h" : "0km/h" }; }
        }

        public CombinedString<decimal> SoC
        {
            get { return new CombinedString<decimal>(DataItem.SoC / 100m) { StringValue = DataItem.SoC + "%" }; }
        }

        public CombinedString<decimal> AvaliableEnergy
        {
            get { return new CombinedString<decimal>(DataItem.AvaliableEnergy) { StringValue = DataItem.AvaliableEnergy + "/40kWh" }; }
        }

        public string EstimatedRange
        {
            get { return "Est.Range: " + DataItem.EstimatedRange + "km"; }
        }

        public string ClimaMode
        {
            get { return DataItem.ClimaState.ToString(); }
        }

        public string MaxChargingPower
        {
            get { return DataItem.MaxPowerThreePhase + "kW"; }
        }

        public string MaxChargePossible
        {
            get { return DataItem.MaxChargePower + "kW"; }
        }

        public CombinedString<float> ChargingPower
        {
            get => new CombinedString<float>((float)DataItem.ChargingPower) { StringValue = DataItem.ChargingPower + "kW" };
        }

        public string TimeToFull
        {
            get
            {
                var hours = DataItem.TimeToFull / 60;
                var minutes = DataItem.TimeToFull - (hours * 60);

                var minuteString = minutes < 10 ? $"0{minutes}" : minutes.ToString();

                return $"-> 100% {hours}:{minuteString}";
            }
        }

        public string TimeToBoarder
        {
            get
            {
                var availableEnergyTarget = DataItem.AvaliableEnergy == 0 || DataItem.SoC == 0 ? 28 : Math.Round(DataItem.AvaliableEnergy / DataItem.SoC * 72m, 1); //default 28 its ~72%;

                var chargingPower = DataItem.ChargingPower;

                if (GlobalDataStore.DataItemManager?.CurrentCharge != null && GlobalDataStore.DataItemManager.CurrentCharge.ChargePoints.Any(x => x.ChargingPointPower > 0))
                {
                    chargingPower = (GlobalDataStore.DataItemManager?.CurrentCharge.ChargePoints.Average(x => x.ChargingPointPower)).Value;
                }

                var result = RemainingChargeTimeCalculator.TryCalculateTimeSpan(availableEnergyTarget, DataItem.AvaliableEnergy, DataItem.ChargingPower);

                return $"-> 72% {result:hh\\:mm}";
            }
        }

        public string PilotSignal
        {
            get
            {
                if (DataItem.PilotAmpere == null)
                    return $"0A -> {DataItem.MaxPowerThreePhase}kW";

                return $"{DataItem.PilotAmpere}A -> {DataItem.MaxPowerThreePhase}kW";
            }
        }

        public float BatteryTemperatureFloat
        {
            get => Convert.ToSingle(DataItem.BatteryTemperature);
        }

        public string BatteryTemperature
        {
            get { return DataItem.BatteryTemperature + "°C"; }
        }

        public string EngineRPMString
        {
            get { return DataItem.EngineRPM + "rpm"; }
        }

        public float EngineRPM
        {
            get => DataItem.EngineRPM.GetValueOrDefault(0);
        }

        public string Speed
        {
            get { return DataItem.Speed + "km/h"; }
        }
        public string Consumption
        {
            get { return DataItem.Consumption + "kW"; }
        }

        public string TripB => "TripB: " + (DataItem.TripB_Distance.HasValue ? DataItem.TripB_Distance.ToString() : "-") + " km";

        public string Odometer
        {
            get
            {
                if (DataItem.TripB_Distance == null)
                {
                    return $"Odometer: {GlobalDataStore.OdoCalculator.LastTripBResetOdometer} km";
                }
                else
                {
                    return $"Odometer: {GlobalDataStore.OdoCalculator.CalculateFromTripBReset(DataItem.TripB_Distance, DataItem.Timestamp.Value)} km";
                }
            }
        }
    }
}