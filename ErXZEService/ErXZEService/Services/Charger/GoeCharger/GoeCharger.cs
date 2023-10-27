using ErXZEService.Services.Arp;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using ErXZEService.Services.Log;

namespace ErXZEService.Services.Charger.GoeCharger
{
    public class GoeCharger : ICharger
    {
        public string ConnectionTo => string.IsNullOrEmpty(Settings.Endpoint) || DataItem.PilotAmpere == 0 ? "not connected" : Settings.Endpoint;

        private GoeChargerSettingItem Settings { get; }

        public Action OnRefreshed;
        private readonly ILogger _logger;

        public string JsonDataItem { get; set; }

        public IChargerDataItem DataItem { get; private set; } = new GoeChargerDataItem();

        private DateTime _lastSet { get; set; }

        public GoeCharger(GoeChargerSettingItem settings, ILogger logger)
        {
            Settings = settings;
            _logger = logger;
        }

        public bool PrepareConnection(object mac, object ips)
        {
            if (FindCharger(mac, ips))
            {
                RefreshState();
                return true;
            }
            return false;
        }

        public bool PauseCharging()
        {
            throw new NotImplementedException();
        }

        public bool RefreshState()
        {
            if (string.IsNullOrEmpty(Settings.Endpoint))
                return false;

            var response = GetHttpResponse(Settings.StatusUrl);

            if (string.IsNullOrEmpty(response))
                return false;
            ApplyJsonResponse(response);

            OnRefreshed?.Invoke();

            return true;
        }

        public bool SetKwhThreshold(int value)
        {
            var dataitem = SetDataItem($"dwo={value}");

            if (dataitem != null)
                DataItem = dataitem;

            return true;
        }

        public bool SetPilotAmpere(int ampere)
        {
            if (ampere > DataItem.MaxDesiredCurrent)
                ampere = DataItem.MaxDesiredCurrent;

            //use amx for temporary setting pa, when persisting pa over a reboot of the charger take amp
            //use this wisely for future development
            var dataitem = SetDataItem($"amx={ampere}");

            if (dataitem != null)
                DataItem = dataitem;

            return DataItem.PilotAmpere == ampere;
        }

        public string GetHttpResponse(string url, string method = "GET", int timeout = 3500)
        {
            var result = string.Empty;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = method;
                request.Timeout = timeout;
                request.ServerCertificateValidationCallback += (senderr, certificate, chain, sslPolicyErrors) => true;
                var response = request.GetResponse();

                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                _logger.LogInformation("HTTP got json:" + result);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Failed to HTTP " + method + " to " + url, e);
            }

            return result;
        }

        private void ApplyJsonResponse(string response)
        {
            JsonDataItem = response;
            DataItem = JsonConvert.DeserializeObject<GoeChargerDataItem>(JsonDataItem);

            if (DataItem != null)
                _logger.LogInformation("Got JsonItem " + response);
        }

        private bool FindCharger(object mac, object ips)
        {
            var expectedIps = new List<string>();

            Task.Run(() =>
            {
                if (ips != null)
                {
                    expectedIps = ((string)ips).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                    foreach (var endpoint in expectedIps)
                    {
                        var charger = new GoeCharger(new GoeChargerSettingItem() { Endpoint = endpoint }, _logger);

                        if (charger.RefreshState())
                        {
                            Settings.Endpoint = charger.Settings.Endpoint;
                            ApplyJsonResponse(charger.JsonDataItem);
                        }
                    }
                }
            });

            if (!FindInArpTable((string)mac))
            {
                Task.Run(() =>
                {
                    var arpTable = ArpProvider.GetArpTableContent();
                    string ipSegment = "";

                    if (arpTable.Count > 0)
                        ipSegment = IpUtils.ExtractIpSegment(arpTable[0].Ip);

                    var ipList = new List<string>();

                    ipList.AddRange(expectedIps);

                    int maxIpEnding = 255;

                    for (int i = 100; i < maxIpEnding; i++)
                        ipList.Add($"{ipSegment}.{i}");

                    for (int i = 1; i < 100; i++)
                        ipList.Add($"{ipSegment}.{i}");

                    var settingsItem = new GoeChargerSettingItem();
                    var ping = new Ping();

                    foreach (var x in ipList)
                    {
                        try
                        {
                            ping.Send(x);

                            if (!string.IsNullOrEmpty(Settings.Endpoint))
                                return;

                            if (FindInArpTable((string)mac))
                                break;
                        }
                        catch
                        {

                        }
                    }

                    var success = FindInArpTable((string)mac);
                });
            }

            return false;
        }

        private bool FindInArpTable(string mac)
        {
            var arpTable = ArpProvider.GetArpTableContent();

            var chargerMapping = arpTable.Find(x => x.Mac == mac);

            if (chargerMapping != null && !string.IsNullOrEmpty(chargerMapping.Ip))
            {
                Settings.Endpoint = chargerMapping.Ip;
                return true;
            }

            return false;
        }

        private GoeChargerDataItem SetDataItem(string payload)
        {
            if (_lastSet.Add(Settings.MinTimeBetweenSetting) > DateTime.Now)
                return null;

            var result = GetHttpResponse(Settings.ChangeUrl + payload, "SET");

            if (string.IsNullOrEmpty(result))
                return null;

            _lastSet = DateTime.Now;
            return JsonConvert.DeserializeObject<GoeChargerDataItem>(result);
        }
    }
}
