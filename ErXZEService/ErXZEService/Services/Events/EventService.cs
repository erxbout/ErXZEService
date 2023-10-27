using ErXZEService.Helper;
using ErXZEService.Models;
using ErXZEService.Services.Log;
using ErXZEService.Services.Paths;
using ErXZEService.Services.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ErXZEService.Services.Events
{
    public class EventService : IEventService
    {
        public List<Event> LatestEvents { get; private set; }

        private ILogger _logger;

        public EventService()
        {
            _logger = IoC.Resolve<ILogger>();
            LoadLatest();
        }

        public void LoadLatest()
        {
            LatestEvents = new List<Event>();

            if (File.Exists(StoragePathProvider.DatabasePath))
            {
                using (var session = new SQLiteSession(_logger))
                {
                    try
                    {
                        var oldestEventTimestamp = DateTime.Now.AddDays(-15);
                        LatestEvents = session.SelectMany<Event>(x => x.Timestamp > oldestEventTimestamp);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Can not load latest events, setting empty list", e);
                    }
                }
            }
            else
            {
                _logger.LogInformation("Can not load latest events because database does not exist yet");
            }
        }

        public void Save()
        {
            using (var session = new SQLiteSession(_logger))
            {
                session.Save(LatestEvents);
            }
        }

        public void PushCarStateChanged(ElectricCarState newState, DateTime? timestamp = null)
        {
            var newEvent = new Event()
            {
                Timestamp = timestamp
            };

            if (newState == ElectricCarState.Charging)
            {
                newEvent.Type = EventType.ChargingStarted;
            }

            if (newState == ElectricCarState.Driving)
            {
                var latestChargingPausedEvent = LatestEvents.LastOrDefault(x => x.Type == EventType.ChargingPaused);
                var latestChargingFinishedEvent = LatestEvents.LastOrDefault(x => x.Type == EventType.ChargingCompleted);

                if (latestChargingPausedEvent != null)
                {
                    if (latestChargingFinishedEvent == null || (latestChargingFinishedEvent != null && latestChargingPausedEvent.Timestamp > latestChargingFinishedEvent.Timestamp))
                    {
                        latestChargingPausedEvent.Type = EventType.ChargingCompleted;
                        latestChargingPausedEvent.Changed = true;

                        Save();
                    }
                }

                newEvent.Type = EventType.TripStarted;
            }

            if (newState == ElectricCarState.Parked)
            {
                var latestChargingEvent = LatestEvents.LastOrDefault(x => x.Type == EventType.ChargingStarted);
                var latestTripEvent = LatestEvents.LastOrDefault(x => x.Type == EventType.TripStarted);

                if (latestChargingEvent != null && latestTripEvent != null && latestChargingEvent.Timestamp > latestTripEvent.Timestamp)
                {
                    newEvent.Type = EventType.ChargingPaused;
                }
                else if (latestTripEvent != null)
                {
                    newEvent.Type = EventType.TripCompleted;
                }
            }

            PushEvent(newEvent);
        }

        public void PushEvent(Event @event)
        {
            if (@event.Timestamp == null)
            {
                @event.Timestamp = DateTime.Now;
            }

            var lastEvent = LatestEvents.LastOrDefault();

            if (lastEvent != null && lastEvent.Type == @event.Type && lastEvent.Timestamp.Value.IsCloseEnough(@event.Timestamp.Value, 5))
            {
                _logger.LogInformation($"Looks like duplicated event, skip that.. Type{@event.Type}, Timestamp{@event.Timestamp}");
            }
            else if(@event.Timestamp > DateTime.Now)
            {
                _logger.LogInformation($"Looks like event is in the future, skip that.. Type{@event.Type}, Timestamp{@event.Timestamp}");
            }
            else
            {
                LatestEvents.Add(@event);
            }
        }
    }
}
