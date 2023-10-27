using System;
using System.Collections.Generic;
using System.Text;

namespace ErXZEService.Services.Charger.GoeCharger
{
    public class GoeChargerSettingItem
    {
        /// <summary>
        /// default 192.168.4.1
        /// my mac 9a:f4:ab:d0:8c:5f
        /// </summary>
        public string Endpoint { get; set; }

        public string BaseUrl => $"http://{Endpoint}/";

        public string StatusUrl => BaseUrl + "status";

        public string ChangeUrl => BaseUrl + "mqtt?payload=";

        public TimeSpan MinTimeBetweenSetting => TimeSpan.FromSeconds(2);
        
        public string GetPayload(Dictionary<string, string> payload)
        {
            var result = new StringBuilder();

            foreach (var item in payload)
            {
                result.Append(item.Key + "=" + item.Value);
            }

            return result.ToString();
        }
    }
}
