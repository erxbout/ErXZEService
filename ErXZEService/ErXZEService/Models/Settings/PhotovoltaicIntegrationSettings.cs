using System.Collections.Generic;

namespace ErXZEService.Models.Settings
{
    public class PhotovoltaicIntegrationSettings
    {
        public bool Enabled { get; set; }

        public List<string> Types { get; set; } = new List<string>() { "None", "Mqtt", "Udp", "Http"};

        public string Type { get; set; } = "None";

        public string FreqencyTopic { get; set; }
    }
}
