using ErXBoutCode.Network;
using ErXZEService.Models;
using ErXZEService.Services;
using ErXZEService.Services.Abrp;
using ErXZEService.Services.CarConnection;
using ErXZEService.Services.Paths;
using ErXZEService.Utils;
using Plugin.LocalNotifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Xamarin.Forms;

namespace ErXZEService.ViewModels
{
    public partial class OverviewViewModel
    {
        public string WifiSequenceNumber { get; set; } = string.Empty;

        private void OnCarConnectionReceive(string message)
        {
            _lastMessageTime = DateTime.Now;
            _lastMessageTimeSource = "Udp";
            var b = CrossLocalNotifications.Current;

            if (!message.Contains("RPM") && !message.Contains("rSOC"))
            {
                //Device.BeginInvokeOnMainThread(() =>
                //{
                //    Toaster?.Short(message);
                //});

                b.Show("New Message", message);
            }

            if (message.Contains("NC"))
                b.Show("No sdcard", "Arduino has no connected sdcard");

            if (message.Contains("NOP"))
            {
                b.Show("No Operation as response", message);
            }
            else if (message.Contains("FCK"))
            {
                b.Show("Fucked up Import", message);
            }
            else if (message.Contains("IMP"))
            {
                if (message.Contains("FIN"))
                {
                    b.Show("Finished Import", message);
                }

                File.AppendAllLines(StoragePathProvider.ImportFilePath, new List<string>() { message.Replace("\r\r\n", "\r\n") });
            }
            else
            {
                string[] splitted = message.Split((Environment.NewLine + "#").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                bool calibrationNeeded = false;

                foreach (var msg in splitted)
                {
                    if (msg == "OK")
                    {
                        _logger.LogInformation($"Received request confirmation.. original answer:{message}");
                        continue;
                    }

                    var indexOfAt = msg.IndexOf('@');
                    if (indexOfAt != -1)
                    {
                        WifiSequenceNumber = msg.Substring(indexOfAt + 1);
                        PropChanged(nameof(WifiSequenceNumber));
                    }

                    try
                    {
                        var messagestring = msg.Replace(Environment.NewLine, "").Replace("/n", "");
                        ElectricCarDataItem line = msg;

                        _logger.LogInformation("Cardata.DataItem before appender: " + CarData.DataItem.ToString());

                        Profiler.MeasureCall(() => GlobalDataStore.DataItemManager.Append(line), "Appendline");

                        _logger.LogInformation(Profiler.GetCallAverageString("Appendline"));
                        _logger.LogInformation("Cardata.DataItem after appender: " + CarData.DataItem.ToString());

                        if (!line.IsFullySpecified)
                        {
                            PropChanged(nameof(CarData));
                            continue;
                        }

                        if (line.Timestamp != null && Math.Abs(line.Timestamp.Value.Ticks - DateTime.Now.Ticks) > 3600000000) //6 mins
                            calibrationNeeded = true;

                        line.Timestamp = DateTime.Now;

                        //Refresh Gui
                        var chargeResult = GlobalDataStore.DataItemManager.LastFinishedCharge;
                        var tripResult = GlobalDataStore.DataItemManager.LastFinishedTrip;

                        GenerateTopics(CarData.DataItem, chargeResult, tripResult);

                        //Save to File
                        File.AppendAllText(StoragePathProvider.NewSendPath, line.ToString() + Environment.NewLine);

                        //GetOverviewData();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Error while decode Message str '{msg}'", e);
                    }
                }

                if (calibrationNeeded)
                    CarConnection.SendRequest(ConnectionRequest.Calibration);

                PropChanged(nameof(CarData));
                PropChanged(nameof(CurrentTripModelItem));
                PropChanged(nameof(CurrentChargeModelItem));
            }

            if (_configuration.UserSettings.AbrpIntegration.Enabled)
            {
                try
                {
                    _telemetryService.SendState(new ElectricCarTelemetryItem()
                    {
                        AvailableEnergy = CarData.DataItem.AvaliableEnergy,
                        BatteryTemperature = CarData.DataItem.BatteryTemperature,
                        EstimatedRange = CarData.DataItem.EstimatedRange,
                        ExternalTemperature = CarData.DataItem.AmbientTemperature.GetValueOrDefault(0),
                        IsCharging = CarData.DataItem.State == ElectricCarState.Charging,
                        IsParked = CarData.DataItem.GearLeverPosition == GearLeverPosition.Park,
                        Power = CarData.DataItem.State == ElectricCarState.Charging ? CarData.DataItem.ChargingPower * -1 : CarData.DataItem.Consumption.GetValueOrDefault(0),
                        Soc = CarData.DataItem.SoC,
                        Speed = CarData.DataItem.Speed.GetValueOrDefault(0)
                    });
                }
                catch(Exception e)
                {
                    _logger.LogError("Telemetry send did not work", e);
                }
            }
        }

        private void OnWifiPolling()
        {
            if (CarData == null || CarData.DataItem == null)
                return;

            var reqType = ConnectionRequest.None;

            // todo: also ask when last change was long ago
            if (CarData.DataItem.MaxChargePower == null)
                reqType = ConnectionRequest.MaxChargePower;
            else if (CarData.DataItem.State == ElectricCarState.none
                    || (CarData.DataItem.State == ElectricCarState.Driving && CarData.DataItem?.Speed == 0)
                    || (CarData.DataItem.State == ElectricCarState.Charging && CarData.DataItem.PilotAmpere == null)
                    || CarData.DataItem.ClimaState == ClimaMode.none
                    || CarData.DataItem.State == ElectricCarState.Parked)
                reqType = ConnectionRequest.State;

            // todo: also ask when last change was long ago -> temp solution here:
            var randomint = new Random().Next(0,10);

            if (randomint > 6)
                reqType = ConnectionRequest.MaxChargePower;

            CarConnection.SendRequest(reqType);
        }

        private void OnConnectionStateChanged(ConnectionType newType)
        {
            ConnectionType = newType;
        }

        private void OnChargerRefreshed()
        {
            PropChanged(nameof(Charger));
            PropChanged(nameof(ProgressVisible));
            PropChanged(nameof(RemainingText));
            PropChanged(nameof(ProgressText));
            PropChanged(nameof(ProgressValue));
        }

        private DateTime _lastPilotAmpereChange;

        private TimeSpan pilotAmpereChangeAdjustmentTimespan = TimeSpan.FromSeconds(15);

        private TimeSpan pilotAmpereChangeDownAdjustmentTimespan = TimeSpan.FromSeconds(7);

        private DateTime _lastOverFreqency;

        private TimeSpan offTime = TimeSpan.FromMinutes(20);

        public string OffTime { get; set; } = "none";

        public int Frequency { get; set; }

        private void PhotovoltaikAdjustReceiver_MessageReceived(string message, IPEndPoint remoteEndPoint, UdpMessageArgs e = null)
        {
            if (_configuration.UserSettings.PhotovoltaicIntegration.Type != "Udp")
            {
                _logger.LogInformation("Received Udp Adjustmessage but settings say to ignore it");
                return;
            }

            if (!message.StartsWith("F01:"))
                return;

            if (Charger == null)
                return;

            try
            {
                var arr = message
                .Split('+')
                .Last()
                .Skip(1)
                .Take(4)
                .ToArray();

                var freqencyString = new StringBuilder().Append(arr).ToString();
                if (int.TryParse(freqencyString, out int freqency))
                {
                    Frequency = freqency;
                    PropChanged(nameof(Frequency));
                }

                AdjustAmpere(freqency);
            }
            catch
            {

            }
        }

        private void AdjustAmpere(int freqency)
        {
            _logger.LogInformation("Received frequency: " + freqency);
            var minAmpere = 7;
            var maxAmpere = Charger.DataItem.IsAdapterInUse ? 16 : Charger.DataItem.MaxCableAmpere;
            var newAmpere = 0;

            if (freqency < 5050)
            {
                if (Charger.DataItem.PilotAmpere > minAmpere)
                    newAmpere = Charger.DataItem.PilotAmpere - 1;
            }
            else if (freqency > 5080)
            {
                if (Charger.DataItem.PilotAmpere < maxAmpere)
                    newAmpere = Charger.DataItem.PilotAmpere + 1;
            }

            if (freqency > 5050 || _lastOverFreqency == DateTime.MinValue)
                _lastOverFreqency = DateTime.Now;

            OffTime = _lastOverFreqency.Add(offTime).ToShortTimeString();
            PropChanged(nameof(OffTime));

            if (freqency > 5200 || _lastPilotAmpereChange.Add(newAmpere < Charger.DataItem.PilotAmpere ? pilotAmpereChangeDownAdjustmentTimespan : pilotAmpereChangeAdjustmentTimespan) > DateTime.Now)
            {
                Charger.RefreshState();
                return;
            }

            if (newAmpere == 0)
                return;

            _logger.LogInformation("attempt changing ampere to: " + newAmpere);

            if (!Charger.SetPilotAmpere(newAmpere))
                return;

            _logger.LogInformation("changed ampere to " + newAmpere);

            Device.BeginInvokeOnMainThread(() =>
            {
                Toaster?.Short("changed ampere" + newAmpere);
            });

            _lastPilotAmpereChange = DateTime.Now;
            PropChanged(nameof(Charger));
        }
    }
}