using ErXZEService.ViewModelItems;
using Plugin.Connectivity;
using System;
using System.Net;
using Xamarin.Forms;

namespace ErXZEService.ViewModels
{
    public enum ConnectionType : byte
    {
        Undefined,
        NotConnected,
        WiFiConnectionNotCompletelyEstablished,
        WiFiConnection,
        InternetConnection
    }

    public partial class OverviewViewModel
    {
        object locker = new object();

        public ContentPage ParentContentPage { get; }

        /// <summary>
        /// If View is still loading this bool is true
        /// </summary>
        private bool _isLoading { get; set; }

        private DateTime LastTopicGenerateTimestamp;

        private string LastKnownIpAddress { get; set; }

        private decimal _avaliableEnergyStart;

        private decimal _tripBStart;

        private decimal _lastTripb;

        private IPAddress CurrentIpAddress
        {
            get
            {
                foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    return address;
                }

                return null;
            }
        }

        private bool IsWiFiConnected => IsConnected(Plugin.Connectivity.Abstractions.ConnectionType.WiFi);

        private bool IsCellularConnected => IsConnected(Plugin.Connectivity.Abstractions.ConnectionType.Cellular);

        private bool IsConnected(Plugin.Connectivity.Abstractions.ConnectionType type)
        {
            var collection = CrossConnectivity.Current.ConnectionTypes;

            foreach (var col in collection)
            {
                if (col == type)
                    return true;
            }
            return false;
        }

        #region dataItems
        public TripModelItem CurrentTripModelItem { get; set; }
        public ChargeModelItem CurrentChargeModelItem { get; set; }
        #endregion
    }
}