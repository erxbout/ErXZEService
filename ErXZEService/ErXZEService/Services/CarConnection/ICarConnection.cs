using ErXZEService.ViewModels;
using System;

namespace ErXZEService.Services.CarConnection
{
    public interface ICarConnection
    {
        Action OnPolling { get; set; }

        Action<string> OnReceive { get; set; }

        Action<ConnectionType> OnConnectionTypeChanged { get; set; }
        bool IsEnabled { get; set; }

        void Poll();
        void SendRequest(ConnectionRequest reqType);
    }
}
