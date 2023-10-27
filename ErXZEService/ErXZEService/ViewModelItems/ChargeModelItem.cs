using ErXBoutCode.MVVM.Property;
using ErXZEService.Helper;
using ErXZEService.Models;
using ErXZEService.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ErXZEService.ViewModelItems
{
    public class ChargeModelItem
    {
        public ChargeModelItem(ChargeItem c)
        {
            _customItem = c;
        }
        public ChargeModelItem()
        {
        }

        private ChargeItem _customItem;

        public ChargeItem Item
        {
            get
            {
                if (_customItem != null)
                    return _customItem;

                return GlobalDataStore.DataItemManager?.CurrentCharge;
            }
        }

        public List<ChargePoint> ChargePoints { get { return Item.ChargePoints; } }

        public CombinedString<DateTime> Date
        {
            get { return new CombinedString<DateTime>(Item.Timestamp) { StringValue = Item.Timestamp.ToString("dd.MM.yyyy") }; }
        }

        public CombinedString<long> Odometer
        {
            get { return new CombinedString<long>(Item.Odometer) { StringValue = $"Odometer: {Item.Odometer}km" }; }
        }

        #region Charge

        public CombinedString<short> ChargedRange => new CombinedString<short>(Item.ChargedRange) { StringValue = $"Charged Range:{Item.ChargedRange}km"};

        public CombinedString<decimal> ChargedKWH => new CombinedString<decimal>(Item.ChargedKWH) { StringValue = $"Charged: {Item.ChargedKWH}kWh" };

        public string ChargedUntilAvaliableEnergy => $"Avaliable Energy after charge: {Item.AvaliableEnergyEnd}kWh";

        public string ChargedSoCString => $" -> {CurrentSocString}";

        public string CurrentSocString => $"SoC: +{Item.EndSoC - Item.StartSoC}% ({Item.StartSoC}-{Item.EndSoC}%)";

        public string ChargedKwhString => Item.Charged;
        public string ChargedSocString => Item.SocCharged;
        public string ChargeRateString => Item.ChargeRate;
        public string ChargedRangeString => Item.RangeCharged;
        public string ChargeTimeString => Item.CurrentChargeTime;
        #endregion

        #region Trips
        private decimal _drivenDistanceSum = -1;

        public decimal DrivenDistanceSum
        {
            get
            {
                if (_drivenDistanceSum == -1)
                    _drivenDistanceSum = Item.Trips.Sum(x => x.DrivenDistance);

                return _drivenDistanceSum;
            }
        }

        private decimal _drivenKWHSum = -1;
        public decimal DrivenKWHSum
        {
            get
            {
                if (_drivenKWHSum == -1)
                    _drivenKWHSum = Item.Trips.Sum(x => x.DrivenKWH);

                return _drivenKWHSum;
            }
        }

        private decimal _avgConsumptionOverTrips = -1;
        public decimal AvgConsumptionOverTrips
        {
            get
            {
                if (_avgConsumptionOverTrips == -1 && DrivenDistanceSum > 0)
                    _avgConsumptionOverTrips = DrivenKWHSum / DrivenDistanceSum * 100;

                return Math.Round(_avgConsumptionOverTrips, 2);
            }
        }

        public string DrivenDistance
        {
            get
            {
                if (Item.IsCurrentCharge)
                    return $"Current distance: {DrivenDistanceSum}km";

                return $"Driven distance: {DrivenDistanceSum}km";
            }
        }

        public string DrivenKWH
        {
            get
            {
                if (Item.IsCurrentCharge)
                    return $"Current Consumption: { DrivenKWHSum }kWh";

                return $"Total consumption: { DrivenKWHSum }kWh";
            }
        }
        public string BreakEvenSoC
        {
            get
            {
                var item = Item.ChargePoints.FirstOrDefault(x => x.ChargingPointPower >= x.MaxChargingPower);
                if (item == null)
                    return "--";
                else
                    return item.SoC.ToString();
            }
        }

        public string AvgConsumption => $"Avg. Consumption: {AvgConsumptionOverTrips}kWh/100km";

        //TODO: 40 durch akkugröße ersetzen
        public string EstimatedFullChargeDistance => $"Est. FullCharge Distance: {Math.Round(40 / AvgConsumptionOverTrips * 100, 2)}km";

        public string ChargePointPower => $"ChargePointPower: {Math.Round(Item.ChargePoints.MaxOrDefault(x => x.ChargingPointPower), 1)}kW";
        public string AvgChargePower => $"Avg. ChargePower: {Math.Round(Item.ChargePoints.AverageOrDefault(x => x.ChargingPower), 1)}kW";

        public string BreakEven => $"BreakEven SoC: {BreakEvenSoC}%";

        public string AmbientTemperature => $"Ambient Temperature: {Math.Round(Item.ChargePoints.AverageOrDefault(x => x.AmbientTemperature), 0)}°C";

        public string MaxBatteryTemperature => $"Max. Battery Temperature: {Item.ChargePoints.MaxOrDefault(x => x.BatteryTemperature)}°C";

        public string MinBatteryTemperature => $"Min. Battery Temperature: {Item.ChargePoints.MinOrDefault(x => x.BatteryTemperature)}°C";

        public string AvgBatteryTemperature => $"Avg. Battery Temperature: {Math.Round(Item.ChargePoints.AverageOrDefault(x => x.BatteryTemperature), 1)}°C";

        public string Losses => $"{LossesInPercent}% ({LossesInKwh}kWh or {Math.Round(PricePerKwh * LossesInKwh, 2)} EUR)";

        public string PricePerKwhString => $"{PricePerKwh} EUR/kWh";
        #endregion

        private decimal PricePerKwh => Item.Cost != 0 && Item.ChargedByBox != 0 ? Math.Round(Item.Cost / Item.ChargedByBox, 2) : 0;

        private decimal LossesInKwh => Item.ChargedByBox < Item.ChargedKWH ? 0 : Math.Round(Item.ChargedByBox - Item.ChargedKWH, 2);

        private int LossesInPercent
        {
            get
            {
                if (Item.ChargedKWH == 0 || Item.ChargedByBox == 0)
                    return 0;

                var percent = 100 - Item.ChargedKWH / Item.ChargedByBox * 100;

                return (int)Math.Round(percent, 0);
            }
        }
    }
}
