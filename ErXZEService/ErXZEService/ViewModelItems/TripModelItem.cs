using ErXBoutCode.MVVM.Property;
using ErXZEService.Models;
using ErXZEService.Services;
using System;

namespace ErXZEService.ViewModelItems
{
    public class TripModelItem
    {
        private TripItem _customItem;

        public TripModelItem()
        {
        }

        public TripModelItem(TripItem c)
        {
            _customItem = c;
        }

        public TripItem Item 
        { 
            get
            {
                if (_customItem != null)
                    return _customItem;

                return GlobalDataStore.DataItemManager?.CurrentTrip;
            }
        }

        public CombinedString<DateTime> Date
        {
            get { return new CombinedString<DateTime>(Item.Timestamp) { StringValue = Item.Timestamp.ToString("dd.MM.yyyy HH:mm") }; }
        }

        #region Stats

        public CombinedString<long> Odometer
        {
            get { return new CombinedString<long>(Item.Odometer) { StringValue = $"Odometer: {Item.Odometer}km" }; }
        }

        public CombinedString<decimal> DrivenDistance
        {
            get { return new CombinedString<decimal>(Item.DrivenDistance) { StringValue = $"Driven Distance: {Item.DrivenDistance}km" }; }
        }

        public CombinedString<decimal> DrivenKWH
        {
            get { return new CombinedString<decimal>(Item.DrivenKWH) { StringValue = $"Consumtion: {Item.DrivenKWH}kWh -> ({Item.Consumption}kWh/100km)" }; }
        }

        public CombinedString<short> MaxSpeed
        {
            get { return new CombinedString<short>(Item.MaxSpeed) { StringValue = $"Max Speed: {Item.MaxSpeed}km/h" }; }
        }

        public string SoCChange
        {
            get { return $"Used SoC: {Item.EndSoC - Item.StartSoC}% ({Item.StartSoC}-{Item.EndSoC}%)"; }
        }

        public string StartBatteryTemperature
        {
            get { return $"Start BatteryTemperature: {Item.StartBatteryTemperature}°C"; }
        }

        public string BatteryTemperatureChange
        {
            get
            {
                string positivePrefix = (Item.EndBatteryTemperature - Item.StartBatteryTemperature) > 0 ? "+" : "";

                return $"BatteryTemperature Change: {positivePrefix}{Item.EndBatteryTemperature - Item.StartBatteryTemperature}°C";
            }
        }

        public string EndBatteryTemperature
        {
            get { return $"End BatteryTemperature: {Item.EndBatteryTemperature}°C"; }
        }

        public string TripDistance => $"Distanz: {Item.CurrentTripDistance}km";

        public string UsedEnergy => $"Energy used: {Item.CurrentUsedEnergy}kWh";

        public string AverageConsumption => $"Avg. consumption: {(Item.CurrentConsumption == 0 ? "-,-- " : Item.CurrentConsumption.ToString())}kWh/100km";
        
        public string EstimatedConsumptionRange => $"Cons.Range: {(Item.RangeWithCurrentConsumption == 0 ? "- " : Item.RangeWithCurrentConsumption.ToString())}km";

        #endregion
    }
}
