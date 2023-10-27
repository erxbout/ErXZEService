using ErXZEService.Services.Log;
using MQTTnet.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ErXZEService.Services.Mqtt
{
    public class MqttService
    {
        public readonly string Wildcard = "#";

        public bool IsConnected => _mqttClient.IsConnected;
        private object connectLock = new object();
        private MqttClient _mqttClient;

        public ILogger Logger { private get; set; }

        public MqttService(string hostname, int port, string username, string password, string clientId = null)
        {
            _mqttClient = new MqttClient(hostname, port, username, password, clientId);
            _mqttClient.ConnectionStateChanged += OnConnectionStateChanged;
            _mqttClient.MessageReceived += OnMessageReceived;
        }

        public List<MqttTopic> Topics { get; set; } = new List<MqttTopic>();

        public Action<string, string> OnAfterMessageReceived { get; set; }

        public Action<bool> OnAfterConnectionStateChanged { get; set; }

        public void Publish(MqttTopic topic, string message)
        {
            if (!_mqttClient.IsConnected)
                return;

            _mqttClient.Publish(topic, message);
        }

        public void Connect(List<MqttTopic> topics = null)
        {
            if (topics != null)
                Topics = topics;

            Reconnect();
        }

        public MqttTopic this[string topic] => Topics.Find(x => x.Topic == topic);

        private void OnMessageReceived(string topic, string message)
        {
            var topicObject = FindClosestTopicObject(topic);

            // skip the first message if specified
            if (topicObject.SkipMessage)
            {
                topicObject.SkipMessage = false;
                return;
            }
            else
            {
                topicObject.OnTopicReceived?.Invoke(message);
                topicObject.OnTopicReceivedInternal?.Invoke(topic, message);
            }

            OnAfterMessageReceived?.Invoke(topic, message);
        }

        private MqttTopic FindClosestTopicObject(string topic)
        {
            var topicObject = this[topic];

            if (topicObject != null)
                return topicObject;

            var wildcardTopics = Topics.Where(x => x.Topic.Contains(Wildcard));

            return wildcardTopics.FirstOrDefault(x => topic.Contains(x.Topic.Replace(Wildcard, string.Empty)));
        }

        private void OnConnectionStateChanged(bool IsConnected)
        {
            if (IsConnected)
                _mqttClient.Subscribe(Topics);
            else
                Reconnect();

            OnAfterConnectionStateChanged?.Invoke(IsConnected);
        }

        private void Reconnect()
        {
            Task.Run(() =>
            {
                lock (connectLock)
                {
                    while (!_mqttClient.IsConnected)
                    {
                        Thread.Sleep(1000);

                        try
                        {
                            _mqttClient.Connect().Wait();
                        }
                        catch (AggregateException ex)
                        {
                            if (ex.InnerExceptions != null && ex.InnerExceptions.Count() == 1 && ex.InnerException is MqttCommunicationException)
                            {
                                // this is empty on purpose to retry connecting on error and not spam log
                            }
                            else
                            {
                                if (Logger != null)
                                {
                                    Logger.LogError("Error while connecting mqtt", ex);
                                }
                            }
                        }

                        Thread.Sleep(1000);
                    }
                }
            });
        }
    }
}
