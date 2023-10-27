using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ErXZEService.Services.Mqtt
{
    public class MqttClient : IMqttClientConnectedHandler, IMqttClientDisconnectedHandler, IMqttApplicationMessageReceivedHandler
    {
        public delegate void MessageReceivedAction(string topic, string message);
        public event MessageReceivedAction MessageReceived;

        public delegate void ConnectionStateChangedAction(bool IsConnected);
        public event ConnectionStateChangedAction ConnectionStateChanged;

        public bool IsConnected => Client.IsConnected;

        private IMqttClientConnectedHandler ConnectedHandler { get; set; }
        private IMqttClientDisconnectedHandler DisconnectedHandler { get; set; }
        private IMqttApplicationMessageReceivedHandler ApplicationMessageReceivedHandler { get; set; }

        private IMqttClient Client { get; set; }

        private IMqttClientOptions Options { get; set; }

        public MqttClient(string hostname, int port, string username, string password, string clientId = null)
        {
            if (clientId == null)
                clientId = "ErX" + nameof(MqttClient) + "_" + Guid.NewGuid().ToString().Split('-')[0];

            var factory = new MqttFactory();
            Client = factory.CreateMqttClient();

            Options = new MqttClientOptionsBuilder()
                .WithCredentials(username, password)
                .WithClientId(clientId)
                .WithTcpServer(hostname, port)
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                    UseTls = true,
                    CertificateValidationHandler = (MqttClientCertificateValidationCallbackContext x) =>
                    {
                        return true;
                    }
                })
                .Build();

            Client.ConnectedHandler = this;
            Client.DisconnectedHandler = this;
            Client.ApplicationMessageReceivedHandler = this;
        }

        public Task Connect()
        {
            return Client.ConnectAsync(Options, CancellationToken.None);
        }

        public Task<MqttClientAuthenticateResult> ConnectAsync()
        {
            return Client.ConnectAsync(Options, CancellationToken.None);
        }

        public void Subscribe(IEnumerable<MqttTopic> topics)
        {
            foreach (var topic in topics)
                Subscribe(topic);
        }

        public async void Subscribe(MqttTopic topic)
        {
            await Client.SubscribeAsync(
                new MqttTopicFilterBuilder()
                .WithTopic(topic.Topic)
                .WithQualityOfServiceLevel(topic.QoS)
                .Build());
        }

        public void Publish(MqttTopic topic, string message)
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic.Topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(topic.QoS)
                .WithRetainFlag(topic.Retained)
                .Build();

            Client.PublishAsync(mqttMessage);
        }

        public Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            return Task.Run(() =>
            {
                ConnectionStateChanged?.Invoke(IsConnected);
            });
        }

        public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            return Task.Run(() =>
            {
                ConnectionStateChanged?.Invoke(IsConnected);
            });
        }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            return Task.Run(() =>
            {
                MessageReceived?.Invoke(eventArgs.ApplicationMessage.Topic, eventArgs.ApplicationMessage.ConvertPayloadToString());
            });
        }
    }
}
