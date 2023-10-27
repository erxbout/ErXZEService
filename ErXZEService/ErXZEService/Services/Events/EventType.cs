namespace ErXZEService.Services.Events
{
    public enum EventType
    {
        Undefined,

        TripStarted = 10,
        TripCompleted = 11,

        ChargingStarted = 20,
        ChargingPaused = 21,
        ChargingCompleted = 22,
    }
}
