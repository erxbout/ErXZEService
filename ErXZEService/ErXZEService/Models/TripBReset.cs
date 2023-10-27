using System;

namespace ErXZEService.Models
{
    public class TripBReset : Entity
    {
        public long Odometer { get; set; }

        public decimal CalculatedOdometer { get; set; }

        public decimal TripB_Distance { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
