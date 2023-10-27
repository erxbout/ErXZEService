using ErXZEService.Models;
using ErXZEService.Services.Mqtt;
using MQTTnet.Protocol;
using MqttServerExtension.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MqttServerExtension
{
    class Program
    {
        private static MqttService _mqttService;

        private static string ParsedValuesMqttBaseTopic = "ErXZEService/values/";

        private static MqttTopic IAmAlive = new MqttTopic("MqttServerExtension/IAmAlive");
        private static MqttTopic TimeTarget = new MqttTopic("ErXZEService/time");
        private static MqttTopic ServerStateTarget = new MqttTopic("ErXZEService/statusWithTime");
        private static string LogFilePath = @"/logs/MqttServerExtension.log";

        private static string LatestState = null;

        private static string CurrentTime
        {
            get
            {
                var now = DateTime.Now;
                return $"{now.Day.ToMinLengthString(2)}.{now.Month.ToMinLengthString(2)}.{now.Year} " +
                    $"{now.Hour.ToMinLengthString(2)}:{now.Minute.ToMinLengthString(2)}:{now.Second.ToMinLengthString(2)}";
            }
        }
        private static string CurrentTimeForArduino
        {
            get
            {
                var now = DateTime.Now;
                return $"{now.Day.ToMinLengthString(2)}.{now.Month.ToMinLengthString(2)}.{now.Year} " +
                    $"{now.Hour.ToMinLengthString(2)}:{now.Minute.ToMinLengthString(2)}";
            }
        }

        static void Main(string[] args)
        {
            InitializeMqtt();

            while (true)
            {
                Thread.Sleep(30000);
                _mqttService.Publish(IAmAlive, $"[{CurrentTime}] -> Still Alive");
            }
        }

        private static void InitializeMqtt()
        {
            var host = Environment.GetEnvironmentVariable("MqttHost");
            var portstring = Environment.GetEnvironmentVariable("MqttPort");
            var username = Environment.GetEnvironmentVariable("MqttUsername");
            var password = Environment.GetEnvironmentVariable("MqttPassword");

            int.TryParse(portstring, out int port);

            LogMessage("MqttConnect", $"Trying to connect to Mqtt via: Host={host}Port={portstring}Username={username}Password=***");

            _mqttService = new MqttService(host, port, username, password);
            _mqttService.OnAfterMessageReceived = LogMessage;
            _mqttService.OnAfterConnectionStateChanged = OnAfterConnectionStateChanged;

            var topics = new List<MqttTopic>()
            {
                new MqttTopic("ErXZEService/status")
                {
                    QoS = MqttQualityOfServiceLevel.ExactlyOnce,
                    OnTopicReceived = x => ServerStateUpdate(x),
                    SkipMessage = true
                },

                new MqttTopic("ErXZEService/statusRequest")
                {
                    QoS = MqttQualityOfServiceLevel.ExactlyOnce,
                    OnTopicReceived = x => ServerStateUpdate(LatestState, true),
                    SkipMessage = true
                },

                new MqttTopic("ErXZEService/timeRequested")
                {
                    QoS = MqttQualityOfServiceLevel.ExactlyOnce,
                    OnTopicReceived = x => _mqttService.Publish(TimeTarget, CurrentTimeForArduino)
                },

                new MqttTopic("ErXZEService/rSOC")
                {
                    QoS = MqttQualityOfServiceLevel.ExactlyOnce
                },

                new MqttTopic("ErXZEService/uSOC")
                {
                    QoS = MqttQualityOfServiceLevel.ExactlyOnce
                }
            };

            _mqttService.Connect(topics);
        }

        private static void OnAfterConnectionStateChanged(bool to)
        {
            var msg = $"[{CurrentTime}] ConnectionState changed to -> {to}";

            if (File.Exists(LogFilePath))
                File.AppendAllLines(LogFilePath, new List<string>() { msg });

            Console.WriteLine(msg);
        }

        private static void LogMessage(string topic, string message)
        {
            var msg = $"[{CurrentTime}] {topic} -> {message}";

            if (File.Exists(LogFilePath))
                File.AppendAllLines(LogFilePath, new List<string>() { msg });

            Console.WriteLine(msg);
        }

        private static void ServerStateUpdate(string state, bool forceSend = false)
        {
            if (state == LatestState && !forceSend)
                LogMessage("serverstateupdate", "state update is the same again");

            if (string.IsNullOrEmpty(state) || (state == LatestState && !forceSend))
                return;

            if (!forceSend)
                LatestState = state + "Dt:" + CurrentTime + ";";

            _mqttService.Publish(ServerStateTarget, LatestState);

            try
            {
                ElectricCarDataItem dataItem = state;

                foreach (var property in dataItem.GetType().GetProperties())
				{
					try
                {
                    if (!property.CanRead || property.PropertyType.IsEnum)
                        continue;

                    var ignoredByMerge = property.GetCustomAttributeOrDefault<IgnoredByMerge>();
                    var ignoredByMqttServerExtension = property.GetCustomAttributeOrDefault<IgnoredByMqttServerExtension>();

                    if (ignoredByMerge != null || ignoredByMqttServerExtension != null)
                        continue;

                    var propertyValue = property.GetValue(dataItem);
                    _mqttService.Publish(ParsedValuesMqttBaseTopic + property.Name, propertyValue.ToString());
                }
					catch (Exception ex)
					{
						LogMessage("Exception", ex.ToString());
					}
				}
            }
            catch (Exception e)
            {
                LogMessage("Exception", e.ToString());
            }
        }
    }
}
