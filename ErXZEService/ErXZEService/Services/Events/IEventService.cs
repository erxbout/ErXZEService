using ErXZEService.Models;
using System;
using System.Collections.Generic;

namespace ErXZEService.Services.Events
{
    public interface IEventService
    {
        List<Event> LatestEvents { get; }

        void LoadLatest();
        void PushCarStateChanged(ElectricCarState newState, DateTime? timestamp = null);
        void Save();
    }
}