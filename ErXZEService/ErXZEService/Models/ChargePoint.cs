using SQLite;
using System;

namespace ErXZEService.Models
{
    [Table(nameof(ChargePoint))]
    public class ChargePoint : Entity
    {
        public long ChargeItemId { get; set; }
        /// <summary>
        /// The SoC value by the Time of ChargePoint
        /// </summary>
        public short SoC { get; set; }

        public decimal AvaliableEnergy { get; set; }
        public short EstimatedRange { get; set; }

        [Ignore]
        public decimal ChargingPower
        {
            get
            {
                decimal value = MaxChargingPower;
                if (ChargingPointPower < MaxChargingPower)
                    value = ChargingPointPower;

                return Math.Round(value, 1);
            }
        }

        /// <summary>
        /// Maximal Charge Power as given by Car BMS
        /// </summary>
        public decimal MaxChargingPower { get; set; }

        /// <summary>
        /// Max Charge Power as given from ChargingPoint
        /// </summary>
        public decimal ChargingPointPower { get; set; }

        public byte PilotAmpere { get; set; }

        #region Temperature Stats
        public short AmbientTemperature { get; set; }
        public short BatteryTemperature { get; set; }
        public DateTime Timestamp { get; set; }
        #endregion
    }
}
