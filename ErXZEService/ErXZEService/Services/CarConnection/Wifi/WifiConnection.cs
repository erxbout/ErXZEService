using ErXBoutCode.Network;
using ErXZEService.Helper;
using ErXZEService.Services.Log;
using ErXZEService.Services.Paths;
using ErXZEService.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ErXZEService.Services.CarConnection.Wifi
{
    public class WifiConnection : ICarConnection
    {
        private const string InitString = "InitializeZOEConnection#";
        private const string CalibrationString = "Calibration";
        private const string AckString = "Ack#";
        private const string ConnectedString = "Connected";

        private readonly ILogger _logger;
        private static ActivityBool _activeInvokation;
        private ErXUdpClient _udp;

        public bool IsEnabled { get; set; }

        private IPEndPoint IpLock { get; set; }

        private DateTime LastPolling { get; set; }

        private DateTime LastBackgroundMessageReceived { get; set; }

        private DateTime LastForegroundMessageReceived { get; set; }

        /// <summary>
        /// Indicates the wifi polling intervall in seconds
        /// </summary>
        public int WifiPollingIntervall { get; set; } = 6;

        public Action OnPolling { get; set; }

        public Action<string> OnReceive { get; set; }

        public Action<ConnectionType> OnConnectionTypeChanged { get; set; }

        private ConnectionType _connectionType;
        public ConnectionType ConnectionType
        {
            get
            {
                return _connectionType;
            }
            set
            {
                _connectionType = value;

                try { OnConnectionTypeChanged?.Invoke(_connectionType); }
                catch (Exception e) { _logger.LogError("Error in invoked method: OnConnectionStateChanged", e); }
            }
        }

        public WifiConnection(int udpPort, ILogger logger)
        {
            _logger = logger;
            _udp = new ErXUdpClient(udpPort);

            _udp.MessageReceived += Udp_MessageReceived;
            _udp.BeforeMessageReceived += Udp_BeforeMessageReceived;

            CommunicationWatchdog();
        }

        public void SendRequest(ConnectionRequest reqType)
        {
            switch (reqType)
            {
                case ConnectionRequest.None:
                    break;
                case ConnectionRequest.Calibration:
                    SendCalibration();
                    break;
                case ConnectionRequest.Import:
                    StartImport();
                    break;
                case ConnectionRequest.State:
                    SendStateRequest();
                    break;
                case ConnectionRequest.MaxChargePower:
                    SendMaxChargeRequest();
                    break;
            }
        }

        private void Udp_BeforeMessageReceived(string message, IPEndPoint remoteEndPoint, UdpMessageArgs e)
        {
            if (e.IsLoopback)
            {
                e.MessageHandled = true;
                return;
            }

            _logger.LogInformation("udp beforemessagereceived:" + message);

            LastBackgroundMessageReceived = DateTime.Now;

            if (IpLock == null)
                IpLock = remoteEndPoint;

            _logger.LogInformation("connection type:" + ConnectionType.ToString());

            if (message.StartsWith(AckString) || message.StartsWith(ConnectedString))
            {
                e.MessageHandled = true;

                if (ConnectionType != ConnectionType.WiFiConnectionNotCompletelyEstablished && ConnectionType != ConnectionType.WiFiConnection)
                    ConnectionType = ConnectionType.WiFiConnectionNotCompletelyEstablished;

                _logger.LogInformation("connection type after ack:" + ConnectionType.ToString());
            }
        }

        private void Udp_MessageReceived(string message, IPEndPoint remoteEndPoint, UdpMessageArgs arg)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.LogInformation("udp messagereceived:" + message);
            try
            {
                if (IpLock != null && remoteEndPoint.ToString() == IpLock.ToString())
                {
                    if (_activeInvokation == null || !_activeInvokation.IsActive)
                    {
                        using (_activeInvokation = new ActivityBool())
                        {
                            SendAck();

                            if (ConnectionType == ConnectionType.WiFiConnectionNotCompletelyEstablished)
                                ConnectionType = ConnectionType.WiFiConnection;

                            LastForegroundMessageReceived = DateTime.Now;
                            OnReceive?.Invoke(message);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Skipping udp message");
                    }
                }

                try
                {
                    File.AppendAllText(StoragePathProvider.LogSendPath, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + message + Environment.NewLine);
                }
                catch
                {

                }
            }
            catch (Exception e)
            {
                _logger.LogError("Udp ThreadError", e);
            }

            stopwatch.Stop();

            _logger.LogInformation("udp messagereceive took: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public void Poll()
        {
            // do nothing because we do not allow polling be triggered from outside in this case
        }

        private void WifiPolling()
        {
            if (IpLock != null)
            {
                if (LastPolling.AddSeconds(WifiPollingIntervall) < DateTime.Now && ConnectionType == ConnectionType.WiFiConnectionNotCompletelyEstablished)
                {
                    OnPolling?.Invoke();
                    LastPolling = DateTime.Now;
                }
            }
        }

        private void SendAck()
        {
            SendRequestInternal(AckString);
        }

        private void StartImport()
        {
            SendRequestInternal("IMP" + "#");
        }

        private void SendCalibration()
        {
            SendRequestInternal(CalibrationString + DateTime.Now.ToString() + "#");
        }

        private void CommunicationWatchdog()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (IsEnabled)
                        {
                            if (LastBackgroundMessageReceived.AddSeconds(15) < DateTime.Now && ConnectionType == ConnectionType.WiFiConnectionNotCompletelyEstablished)
                                ConnectionType = ConnectionType.NotConnected;

                            if (LastBackgroundMessageReceived.AddSeconds(10) < DateTime.Now && ConnectionType == ConnectionType.WiFiConnectionNotCompletelyEstablished)
                                BeginWiFiConnection();

                            if (LastForegroundMessageReceived.AddSeconds(5) < DateTime.Now && ConnectionType == ConnectionType.WiFiConnection)
                                ConnectionType = ConnectionType.WiFiConnectionNotCompletelyEstablished;

                            if (ConnectionType != ConnectionType.WiFiConnection && ConnectionType != ConnectionType.WiFiConnectionNotCompletelyEstablished)
                            {
                                //ConnectionTries++;
                                BeginWiFiConnection();
                            }

                            Thread.Sleep(1000);

                            WifiPolling();
                        }

                        Thread.Sleep(1000);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Exception in CommunicationWatchdog!", e);
                    }
                }
            });
        }


        private void BeginWiFiConnection()
        {
            if (ConnectionType == ConnectionType.NotConnected)
                IpLock = null;

            try
            {
                _udp.broadcast(InitString);
                //_udp.send(InitString, new IPEndPoint(IPAddress.Parse("192.168.1.1"), UdpPort));
                //_udp.send(InitString, new IPEndPoint(IPAddress.Parse("192.168.43.125"), UdpPort));
            }
            catch (SocketException e)
            {
                _logger.LogError("network error on begin wifi connection " + e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("error on begin wifi connection", e);
            }
        }

        //Request commands.length must be <= 3
        private void SendStateRequest()
        {
            SendRequestInternal("ST" + "#");
        }

        private void SendMaxChargeRequest()
        {
            SendRequestInternal("MxC" + "#");
        }

        private void SendRequestInternal(string message)
        {
            if (IpLock != null)
                _udp.send(message, IpLock);
        }
    }
}