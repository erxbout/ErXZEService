using ErXZEService.Services;
using ErXZEService.Services.SQL.Attributes;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ErXZEService.Models
{
    [Table(nameof(TripItem))]
    public class TripItem : Entity, ITopicModelItem
    {
        public long ChargeItemId { get; set; }

        public long TripItemGroupId { get; set; }

        public DateTime Timestamp { get; set; }

        private string _tripCaption;

        public string Caption
        {
            get
            {
                return _tripCaption;
            }
            set
            {
                if (value != _tripCaption)
                    Changed = true;
                _tripCaption = value;
            }
        }

        public string SubCaption => $"-{DrivenKWH} kWh -> {DrivenDistance} km ({Consumption}kWh/100km)";

        public string MonthCaption => $"{Timestamp.Day}.{Timestamp.Month}";

        [Ignore]
        [OneToMany(nameof(TripPoint.TripItemId))]
        public List<TripPoint> TripPoints { get; set; } = new List<TripPoint>();

        #region Driving
        public decimal DrivenKWH { get; set; }

        public decimal DrivenDistance { get; set; }

        public byte MaxSpeed { get; set; }
        #endregion

        #region Battery
        public short StartSoC { get; set; }

        public short EndSoC { get; set; }

        public decimal StartAvaliableEnergy { get; set; }

        public decimal EndAvaliableEnergy { get; set; }

        public short? StartBatteryTemperature { get; set; }

        public short? EndBatteryTemperature { get; set; }
        #endregion

        #region TripB
        public decimal TripB_AverageConsumption { get; set; }

        public decimal? TripB_Distance { get; set; }

        public decimal? StartTripB_Distance { get; set; }

        public long Odometer { get; set; }

        public long? CalculatedOdometer
        {
            get
            {
                if (TripB_Distance != null)
                    return GlobalDataStore.OdoCalculator.CalculateFromTripBReset(TripB_Distance, Timestamp);

                return null;
            }
        }
        #endregion

        #region Calculated

        /// <summary>
        /// For calculation of Consumption while driving (Current Available Energy)
        /// </summary>
        [Ignore]
        public decimal AvailableEnergy => TripPoints.LastOrDefault(x => x.AvaliableEnergy > 0)?.AvaliableEnergy ?? 0;

        /// <summary>
        /// For calculation of Consumption while driving (Available Energy at start)
        /// </summary>
        [Ignore]
        public decimal AvailableEnergyBegin => TripPoints.FirstOrDefault(x => x.AvaliableEnergy > 0)?.AvaliableEnergy ?? 0;

        [Ignore]
        public decimal TripDistanceBegin => TripPoints.FirstOrDefault(x => x.TripB_Distance > 0)?.TripB_Distance ?? 0;

        [Ignore]
        public decimal TripDistance => TripPoints.LastOrDefault(x => x.TripB_Distance > 0)?.TripB_Distance ?? 0;

        [Ignore]
        public decimal CurrentTripDistance => TripDistance - TripDistanceBegin;

        [Ignore]
        public byte CurrentMaxSpeed => TripPoints.Count > 0 ? TripPoints.Max(x => x.Speed) : byte.MinValue;

        [Ignore]
        public decimal Consumption
        {
            get
            {
                if (DrivenDistance == 0)
                    return 0;

                return Math.Round(DrivenKWH / DrivenDistance * 100, 2);
            }
        }

        /// <summary>
        /// Consumption while driving
        /// </summary>
        [Ignore]
        public decimal CurrentConsumption
        {
            get
            {
                // start to show data after driving at least 2km
                if (CurrentTripDistance < 2)
                    return 0;

                return Math.Round(CurrentUsedEnergy / CurrentTripDistance * 100, 2);
            }
        }

        /// <summary>
        /// Consumption while driving
        /// </summary>
        [Ignore]
        public decimal RangeWithCurrentConsumption
        {
            get
            {
                if (CurrentConsumption == 0 || AvailableEnergy == 0)
                    return 0;

                return Math.Round(AvailableEnergy / CurrentConsumption * 100, 0);
            }
        }

        [Ignore]
        public decimal CurrentUsedEnergy
        {
            get
            {
                if (AvailableEnergyBegin == 0)
                    return 0;

                return AvailableEnergyBegin - AvailableEnergy;
            }
        }
        #endregion

        [Ignore]
        public string NextChargePointId { get; set; }
		[Ignore]
		public bool NextChargePointAvailable { get; set; }
		[Ignore]
		public string NextChargePointAvailableStatus { get; set; }

		public void ApplyStartAndEnd()
        {
            var firstItem = TripPoints.FirstOrDefault();
            var lastItem = TripPoints.LastOrDefault();

            if (firstItem == null || lastItem == null)
                return;

            StartSoC = firstItem.SoC;
            StartAvaliableEnergy = firstItem.AvaliableEnergy;
            StartBatteryTemperature = firstItem.BatteryTemperature;
            StartTripB_Distance = firstItem.TripB_Distance;

            EndSoC = lastItem.SoC;
            EndAvaliableEnergy = lastItem.AvaliableEnergy;
            EndBatteryTemperature = lastItem.BatteryTemperature;
            TripB_Distance = lastItem.TripB_Distance;
        }

        public override string ToString()
        {
            return $"-{DrivenKWH} -> {DrivenDistance} ({Consumption}kWh/100km)";
        }
    }
}
