using SQLite;

namespace ErXZEService.Models
{
    [Table(nameof(TripPoint))]
    public class TripPoint : Entity
    {
        public long TripItemId { get; set; }

        public decimal AvaliableEnergy { get; set; }
        public short SoC { get; set; }

        public short BatteryTemperature { get; set; }

        #region TripB
        public decimal TripB_AverageConsumption { get; set; }

        public decimal TripB_Distance { get; set; }

        public short TripB_UsedBattery { get; set; }

        public decimal TripB_AverageSpeed { get; set; }
        #endregion
        
        public byte CCMode { get; set; }

        [Ignore]
        public byte Speed { get; set; }
    }
}
