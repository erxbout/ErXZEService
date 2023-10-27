namespace ErXZEService.Services.Charger
{
    public interface ICharger
    {
        IChargerDataItem DataItem { get; }

        bool SetPilotAmpere(int ampere);

        bool PrepareConnection(object parameter, object otherparameter);

        bool PauseCharging();

        bool RefreshState();
    }
}
