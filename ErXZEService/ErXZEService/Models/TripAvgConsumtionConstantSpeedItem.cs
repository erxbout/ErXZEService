using SQLite;

namespace ErXZEService.Models
{
    [Table(nameof(TripAvgConsumptionConstantSpeedItem))]
    public class TripAvgConsumptionConstantSpeedItem : Entity
    {
        public string Consumption { get; set; }

        public short CCSpeed { get; set; }

        public decimal Distance { get; set; }
    }
}
