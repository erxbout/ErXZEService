namespace ErXZEService.Services.Charger
{
    public interface IChargerDataItem
    {
        int PilotAmpere { get; set; }

        int MaxCableAmpere { get; set; }
        int MaxDesiredCurrent { get; }

        bool IsAdapterInUse { get; }

        decimal CurrentChargeKwh { get; }

        decimal LivetimeChargedKwh { get; }

        decimal KwhThreshold { get; }
    }
}
