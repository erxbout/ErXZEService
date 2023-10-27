using ErXZEService.Helper;
using ErXZEService.Models;
using ErXZEService.Services.CarDataPersistence;
using ErXZEService.Services.Log;
using ErXZEService.Services.Mqtt;
using ErXZEService.ViewModels;
using System;

namespace ErXZEService.Services.CarConnection.Mqtt
{
    public class MqttConnection : ICarConnection
    {
        private readonly ILogger _logger;
        private readonly MqttService _mqttService;
        private readonly ElectricCarDataItemManager _electricCarDataItemManager;
        private DateTime _lastPoll;
        private MqttTopic _pollingTopic = "ErXZEService/statusRequest";
        private MqttTopic _stateReceiveTopic;

        public Action OnPolling { get; set; }

        public Action<string> OnReceive { get; set; }
        public Action<ConnectionType> OnConnectionTypeChanged { get; set; }

        public bool IsEnabled { get; set; } = true;

        public void SendRequest(ConnectionRequest reqType)
        {
            // we do not send requests over server yet
        }

        public MqttConnection(ILogger logger, ref MqttService mqttService, ref ElectricCarDataItemManager electricCarDataItemManager)
        {
            _logger = logger;
            _mqttService = mqttService;
            _electricCarDataItemManager = electricCarDataItemManager;
            _stateReceiveTopic = _mqttService["ErXZEService/statusWithTime"];
            _stateReceiveTopic.OnTopicReceived = ReceiveServerUpdate;
        }

        public void Poll()
        {
            try
            {
                if (_mqttService.IsConnected && _lastPoll.IsTimespanElapsed(TimeSpan.FromMinutes(4)))
                {
                    _mqttService.Publish(_pollingTopic, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                    _lastPoll = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("General error while fetching data from server", ex);
            }
        }

        private void ReceiveServerUpdate(string response)
        {
            if (!IsEnabled)
                return;

            var lines = response.Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var informationChanged = false;

            foreach (var line in lines)
            {
                try
                {
                    ElectricCarDataItem converted = line.Replace("\r", "");

                    if (converted.Timestamp > _electricCarDataItemManager.CurrentDate)
                    {
                        _electricCarDataItemManager.Append(converted);

                        if (converted.Timestamp != null)
                        {
                            OnReceive?.Invoke(line);
                        }

                        informationChanged = true;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("General error while updating dataitem from server", e);
                }
            }

            if (informationChanged)
            {
                OnConnectionTypeChanged?.Invoke(ConnectionType.InternetConnection);
            }
        }
    }
}
