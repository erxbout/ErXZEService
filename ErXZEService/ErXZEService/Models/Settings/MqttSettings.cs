using ErXZEService.Services.Mqtt;
using MQTTnet.Client.Connecting;
using System;
using System.Threading.Tasks;

namespace ErXZEService.Models.Settings
{
    public class MqttSettings
    {
        public Action OnChangeSettings { get; set; }

        public MqttSettings()
        {
            HostnameOrIp = "";
            Port = 1883;
        }

        private bool _enabled;
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                OnChangeSettings?.Invoke();
            }
        }

        public string HostnameOrIp { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public StateViewItem TestState { get; set; }

        public void TestSettings()
        {
            Task.Run(async () =>
            {
                var client = new MqttClient(HostnameOrIp, Port, Username, Password);
                MqttClientAuthenticateResult result = null;

                try { result = await client.ConnectAsync(); } catch { }

                var testStateEnum = result?.ResultCode == MqttClientConnectResultCode.Success
                    ? Settings.TestState.Success
                    : Settings.TestState.Fail;

                TestState = SettingsDataItem.GetState(testStateEnum);

                OnChangeSettings();
            });
        }
    }
}
