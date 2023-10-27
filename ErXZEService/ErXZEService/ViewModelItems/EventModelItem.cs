using System;

namespace ErXZEService.Services.Events
{
    public class EventModelItem
    {
        public Event EventItem { get; }

        public EventModelItem(Event eventItem)
        {
            EventItem = eventItem;
        }

        public EventType Type => EventItem.Type;

        public DateTime? Timestamp => EventItem.Timestamp;

        public string Comment => EventItem.Comment;

        public string ImageSource
        {
            get
            {
                switch (EventItem.Type)
                {
                    case EventType.Undefined:
                        break;
                    case EventType.TripStarted:
                        return "ElectricCarDriving.png";
                    case EventType.TripCompleted:
                        return "ElectricCarDriving.png";
                    case EventType.ChargingStarted:
                        return "BatteryChargingSymbol.png";
                    case EventType.ChargingPaused:
                        return "BatteryChargingSymbol.png";
                    case EventType.ChargingCompleted:
                        return "BatteryChargingSymbol.png";
                }

                return "Fail.png";
            }
        }

        public string Caption => EventItem.ToString();
    }
}
