using MQTTnet.Protocol;
using System;

namespace ErXZEService.Services.Mqtt
{
    public class MqttTopic
    {
        public MqttTopic(string topic, MqttQualityOfServiceLevel qos)
        {
            Topic = topic;
            QoS = qos;
        }

        public MqttTopic(string topic) : this(topic, MqttQualityOfServiceLevel.AtLeastOnce)
        {
        }

        /// <summary>
        /// skip the next message if set true
        /// </summary>
        public bool SkipMessage { get; set; }

        public bool Retained { get; set; }

        public string Topic { get; }

        public Action<string> OnTopicReceived { get; set; }

        public Action<string, string> OnTopicReceivedInternal { get; set; }

        public MqttQualityOfServiceLevel QoS { get; set; }

        public static implicit operator MqttTopic(string topic)
        {
            return new MqttTopic(topic);
        }
    }
}
