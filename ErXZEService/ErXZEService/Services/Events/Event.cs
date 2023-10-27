using ErXZEService.Models;
using SQLite;
using System;

namespace ErXZEService.Services.Events
{
    [Table(nameof(Event))]
    public class Event : Entity
    {
        public EventType Type { get; set; }

        public DateTime? Timestamp { get; set; }

        public string Comment { get; set; }

        public override string ToString()
        {
            return $"{Type}@{Timestamp}_{Comment}";
        }
    }
}
