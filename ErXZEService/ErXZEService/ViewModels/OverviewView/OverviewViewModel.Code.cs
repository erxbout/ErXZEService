using DryIoc;
using ErXBoutCode.MVVM;
using ErXBoutCode.Network;
using ErXZEService.Converter;
using ErXZEService.DependencyServices;
using ErXZEService.Models;
using ErXZEService.Models.Settings;
using ErXZEService.Services;
using ErXZEService.Services.Abrp;
using ErXZEService.Services.CarConnection;
using ErXZEService.Services.CarConnection.Mqtt;
using ErXZEService.Services.CarConnection.Wifi;
using ErXZEService.Services.CarDataPersistence;
using ErXZEService.Services.ChargepointPolling.Interfaces;
using ErXZEService.Services.Charger;
using ErXZEService.Services.Charger.GoeCharger;
using ErXZEService.Services.Configuration;
using ErXZEService.Services.Events;
using ErXZEService.Services.Log;
using ErXZEService.Services.Mqtt;
using ErXZEService.Services.Paths;
using ErXZEService.Services.SQL;
using ErXZEService.Utils;
using ErXZEService.Views;
using ErXZEService.Views.Overview;
using MQTTnet.Protocol;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ErXZEService.ViewModels
{
    public partial class OverviewViewModel
    {
        public ICharger Charger { get; set; }

        public IToaster Toaster { get; set; }

        public ICarConnection CarConnection { get; set; }
        public ICarConnection OnlineCarConnection { get; set; }

        private ErXUdpClient _photovoltaikAdjustReceiver;

        private MqttService _mqttService;
        private ILogger _logger;
        private IAbrpTelemetryService _telemetryService;
		private IChargepointPoller _chargepointPoller;
        private IReadonlyConfiguration _configuration;
        private IEventService _eventService;

        public OverviewViewModel(ContentPage parent)
        {
            _logger = IoC.Resolve<ILogger>();
            _telemetryService = IoC.Resolve<IAbrpTelemetryService>();
			_chargepointPoller = IoC.Resolve<IEnergieSteiermarkChargepointPoller>();
            _configuration = IoC.Resolve<IReadonlyConfiguration>();
            _eventService = IoC.Resolve<IEventService>();

			ConnectionType = ConnectionType.NotConnected;

			_isLoading = true;
			ParentContentPage = parent;

            Initialize();

            DebugChangeState = new ActionCommand(() =>
            {
                CarData.DataItem.StateNumber += 1;

                if (CarData.DataItem.StateNumber > (byte)ElectricCarState.Charging)
                    CarData.DataItem.State = ElectricCarState.Parked;

                if (CarData.DataItem.State == ElectricCarState.none)
                    CarData.DataItem.State = ElectricCarState.Parked;

                PropChanged(nameof(CarData));
                GenerateTopics(CarData.DataItem, GlobalDataStore.DataItemManager.LastFinishedCharge, GlobalDataStore.DataItemManager.LastFinishedTrip);
            });

            ShowTirePressures = new ActionCommand(() =>
            {
                Page myPage = new TirePressureView();
                myPage.BindingContext = this;
                myPage.Title = "TirePressure";

                parent.Navigation.PushAsync(myPage);
            });

            ReImport = new ActionCommand(ReImport_Action);
            //Import = new ActionCommand(StartImport);
            ChangeThreshold = new ActionCommand(OnChangeThreshold);
            ChangeDesiredCurrent = new ActionCommand(OnChangeDesiredCurrent);

            Toaster = DependencyService.Get<IToaster>();
            _logger.LogInformation("-------------- Application Started ---------------");
        }

        private new void PropChanged([CallerMemberName] string propertyName = null)
        {
            //Log.Info($"PropertyChange of <{propertyName}>");
            base.PropChanged(propertyName);
        }

        private async void OnChangeThreshold()
        {
            var result = await ParentContentPage.DisplayPromptAsync("Enter new Threshold", "Multiply kWh with 10 before!", keyboard: Keyboard.Numeric);

            if (result == null)
                return;

            int.TryParse(result, out int newthreshold);

            if (Charger is GoeCharger g)
                g.SetKwhThreshold(newthreshold);

            PropChanged(nameof(ThresholdButtonText));
        }

        private async void OnChangeDesiredCurrent()
        {
            var result = await ParentContentPage.DisplayPromptAsync("Enter new Current", "Value in Ampere (A)", keyboard: Keyboard.Numeric);

            if (result == null)
                return;

            int.TryParse(result, out int newthreshold);

            if (Charger is GoeCharger g)
                g.SetPilotAmpere(newthreshold);
        }

        private async Task PermissionRequest()
        {
            try
            {
                var storagePermission = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

                if (storagePermission != PermissionStatus.Granted && storagePermission != PermissionStatus.Restricted)
                {
                    var newState = await Device.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.StorageWrite>());

                    if (newState != PermissionStatus.Granted)
                    {
                        //idioto
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Permissionrequest failed ", e);
            }
        }

        private void InitializeMqtt()
        {
            _mqttService = new MqttService(_configuration.UserSettings.Mqtt.HostnameOrIp, _configuration.UserSettings.Mqtt.Port, _configuration.UserSettings.Mqtt.Username, _configuration.UserSettings.Mqtt.Password);

            var topics = new List<MqttTopic>()
            {
                new MqttTopic("ErXZEService/statusWithTime")
                {
                    QoS = MqttQualityOfServiceLevel.ExactlyOnce
                },
                "ErXZEService/time"
            };

            if (_configuration.UserSettings.PhotovoltaicIntegration.Enabled
                && _configuration.UserSettings.PhotovoltaicIntegration.Type == "Mqtt"
                && !string.IsNullOrEmpty(_configuration.UserSettings.PhotovoltaicIntegration.FreqencyTopic))
                topics.Add(new MqttTopic(_configuration.UserSettings.PhotovoltaicIntegration.FreqencyTopic)
                {
                    OnTopicReceived = s =>
                    {
                        if (int.TryParse(s, out int freqency))
                        {
                            Frequency = freqency;
                            PropChanged(nameof(Frequency));
                        }

                        AdjustAmpere(freqency);
                    }
                });

            _mqttService.OnAfterMessageReceived = (topic, message) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Toaster?.Short("mqtt@" + topic + ": " + message);
                });
            };

            _mqttService.OnAfterConnectionStateChanged = (isNowConnected) =>
            {
            };

            _mqttService.Connect(topics);

            OnlineCarConnection = new MqttConnection(_logger, ref _mqttService, ref GlobalDataStore.DataItemManager);
            OnlineCarConnection.OnConnectionTypeChanged = x =>
            {
                if (ConnectionType == ConnectionType.NotConnected)
                {
                    ConnectionType = x;
                    _lastMessageTimeSource = "Data";
                    _lastMessageTime = GlobalDataStore.DataItemManager.CurrentDataItem.Timestamp.Value;

                    GetOverviewData();
                    PropChanged(nameof(CurrentTripModelItem));
                    PropChanged(nameof(CurrentChargeModelItem));
                }
            };
        }

        private void ReImport_Action()
        {
            if (!File.Exists(StoragePathProvider.TodaysDatabasePath))
            {
                File.Move(StoragePathProvider.DatabasePath, StoragePathProvider.TodaysDatabasePath);

                if (File.Exists(StoragePathProvider.NewSendPath))
                    File.Move(StoragePathProvider.NewSendPath, StoragePathProvider.NewSendPath.Replace(".TXT", DateTime.Now.ToShortDateString() + ".TXT"));

                GlobalDataStore.DataItemManager.AppendingEnabled = false;

                Toaster.Short("Ready for ReImport");
            }
        }

        private void AddLine_CheckCCMode(ElectricCarDataItem line)
        {
            if (line.CCMode != null)
            {
                if (line.CCMode == 4)
                {
                    if (_avaliableEnergyStart == 0)
                    {
                        _avaliableEnergyStart = line.AvaliableEnergy;
                        _tripBStart = _lastTripb;
                    }

                    CruiseControlActive = true;
                }
                else
                {
                    CruiseControlActive = false;

                    if (AdditionalInfoLabelText.Contains("100km"))
                    {
                        var avgConsumptionItem = new TripAvgConsumptionConstantSpeedItem()
                        {
                            CCSpeed = CarData.CruiseControlSpeed.Value,
                            Consumption = AdditionalInfoLabelText,
                            Distance = _lastTripb - _tripBStart
                        };

                        avgConsumptionItem.Save();

                        AdditionalInfoLabelText = "";
                        _avaliableEnergyStart = 0;
                    }
                }
                PropChanged(nameof(CruiseControlActive));
                PropChanged(nameof(CarData.CruiseControlImageSource));
            }


            if (line.TripB_Distance != null)
                _lastTripb = line.TripB_Distance.Value;

            if (CarData != null && CarData.DataItem != null && CarData.DataItem.State == ElectricCarState.Driving)
            {
                if (_avaliableEnergyStart != 0 && _tripBStart != 0)
                {
                    var energyDif = _avaliableEnergyStart - line.AvaliableEnergy;
                    var tripbdif = _lastTripb - _tripBStart;

                    var consumption = Math.Round((energyDif / tripbdif) * 100, 2);

                    var consumptionStr = consumption + "kWh/100km";

                    if (consumptionStr != AdditionalInfoLabelText)
                    {
                        AdditionalInfoLabelText = consumptionStr;
                    }
                }
            }
        }

        public void PushNavigationToLiveView(ElectricCarState state, Page parentpage)
        {
            var lastPage = parentpage.Navigation.NavigationStack.LastOrDefault();
            Page myPage = null;

            if (state == ElectricCarState.Driving)
            {
                myPage = new DrivingLiveView();
            }
            else if (state == ElectricCarState.Charging)
            {
                myPage = new ChargingLiveView();

                Charger = new GoeCharger(new GoeChargerSettingItem(), _logger);
                // todo: remove arp requesting as its not working any more in android version 8+
                Charger.PrepareConnection("00:00:00:00:00:00", _configuration.UserSettings.Charger.PossibleChargerAddresses);
                PropChanged(nameof(Charger));
            }

            if (state == ElectricCarState.Parked || (myPage != null && parentpage.Navigation.NavigationStack.Count > 1 && lastPage.GetType() != myPage.GetType()))
            {
                Device.BeginInvokeOnMainThread(() => parentpage.Navigation.PopToRootAsync(true));
            }

            if (myPage != null)
            {
                if (lastPage != null && lastPage.GetType() != myPage.GetType())
                {
                    myPage.BindingContext = this;
                    myPage.Title = "LiveView";
                }

                Device.BeginInvokeOnMainThread(() => parentpage.Navigation.PushAsync(myPage, true));
            }
        }

        public void Timer()
        {
            Task.Run(async () =>
            {
                while (true)
                {
					if (_configuration.UserSettings.ChargepointIdPolling.Enabled)
					{
						try
						{
							var result = await _chargepointPoller.Poll(CurrentTripModelItem.Item.NextChargePointId);

							if (result != null && result.Success)
							{
								CurrentTripModelItem.Item.NextChargePointAvailable = result.IsAvailable;
								CurrentTripModelItem.Item.NextChargePointAvailableStatus = result.AvailableStatus;

								PropChanged(nameof(TripItem.NextChargePointAvailable));
								PropChanged(nameof(TripItem.NextChargePointAvailableStatus));
							}
						}
						catch (Exception e)
						{
							_logger.LogError("Charger polling did not work", e);
						}
					}

                    if (CarData != null && CarData.DataItem != null && CarData.DataItem.State == ElectricCarState.Charging && !ChargeTimerStarted)
                    {
                        ChargeTimerStarted = true;
                        _ = Task.Run(async () =>
                        {
                            _logger.LogInformation("Start chargetimer thread");
                            while (CarData != null && CarData.DataItem != null && CarData.DataItem.State == ElectricCarState.Charging)
                            {
                                PropChanged(nameof(CurrentChargeModelItem));
                                await Task.Delay(1000);
                            }
                            _logger.LogInformation("Stop chargetimer thread");
                        });
                    }

                    if (CarData != null && CarData.DataItem != null && CarData.DataItem.State != ElectricCarState.Charging)
                    {
                        ChargeTimerStarted = false;
                    }

                    await Task.Delay(1000);

                    if (CurrentIpAddress.ToString() != LastKnownIpAddress)
                    {
                        LastKnownIpAddress = CurrentIpAddress.ToString();
                    }

                    try
                    {
                        if (IsWiFiConnected)
                        {
                            if (ConnectionType != ConnectionType.WiFiConnection && ConnectionType != ConnectionType.WiFiConnectionNotCompletelyEstablished)
                                OnlineCarConnection?.Poll();

                            await Task.Delay(2000);

                            ChargerPolling();
                        }
                        else
                        {
                            OnlineCarConnection?.Poll();
                            await Task.Delay(2000);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Timer Failed", e);
                    }
                }
            });
        }

        public Task Initialize()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await PermissionRequest();

                    CarConnection = new WifiConnection(1905, _logger)
                    {
                        OnConnectionTypeChanged = new Action<ConnectionType>(OnConnectionStateChanged),
                        OnPolling = OnWifiPolling,
                        OnReceive = OnCarConnectionReceive
                    };

                    _photovoltaikAdjustReceiver = new ErXUdpClient(1906);
                    _photovoltaikAdjustReceiver.MessageReceived += PhotovoltaikAdjustReceiver_MessageReceived;

                    GlobalDataStore.DataItemManager = new ElectricCarDataItemManager(_logger)
                    {
                        OnImportProgressChange = x =>
                        {
                            LastMessageTime = x;
                        },
                        OnCarStateChanged = x =>
                        {
                            _logger.LogInformation($"Car state changed to: {x}");

                            _eventService.PushCarStateChanged(x);
                            _eventService.Save();

                            if (_configuration.UserSettings.General.AutoNavigateToLiveView)
                            {
                                PushNavigationToLiveView(x, ParentContentPage);
                            }

                            if (_configuration.UserSettings.AbrpIntegration.Enabled && _configuration.UserSettings.AbrpIntegration.AutoDisableTelemetryChargeCount > 0)
                            {
                                if (x == ElectricCarState.Charging)
                                {
                                    var writableConfiguration = IoC.Resolve<IConfiguration>();
                                    writableConfiguration.UserSettings.AbrpIntegration.AutoDisableTelemetryChargeCount--;

                                    if (writableConfiguration.UserSettings.AbrpIntegration.AutoDisableTelemetryChargeCount == 0)
                                    {
                                        writableConfiguration.UserSettings.AbrpIntegration.Enabled = false;
                                    }
                                    writableConfiguration.Save();
                                }
                            }
                        },
                        ReverseEngineer = true
                    };

                    if (File.Exists(StoragePathProvider.DatabasePath))
                    {
                        using (var session = new SQLiteSession(_logger))
                        {
                            GlobalDataStore.DataItemManager.LoadOverviewData(session);

                            GetOverviewData();

                            GlobalDataStore.DataItemManager.Load(session);
                        }
                    }
                    else
                    {
                        GlobalDataStore.DataItemManager.Load(StoragePathProvider.BaseStoragePath);
                    }

                    //todo: make this better
                    CurrentTripModelItem = new ViewModelItems.TripModelItem();
                    CurrentChargeModelItem = new ViewModelItems.ChargeModelItem();
                    GetOverviewData();
                    PropChanged(nameof(GlobalDataStore.DataItemManager.ChargeItems));
                    PropChanged(nameof(GlobalDataStore.DataItemManager.TripItems));
                    PropChanged(nameof(CurrentTripModelItem));
                    PropChanged(nameof(CurrentChargeModelItem));

                    _isLoading = false;
                    CarConnection.IsEnabled = true;

                    InitializeMqtt();
                    Timer();
                }
                catch (Exception e)
                {
                    LastMessageTime = "Error while loading";
                    _logger.LogError("Error while loading", e);
                }
            });
        }

        public void GetOverviewData()
        {
            Task.Run(() =>
            {
                var result = GlobalDataStore.DataItemManager.CurrentDataItem;
                var chargeResult = GlobalDataStore.DataItemManager.LastFinishedCharge;
                var tripResult = GlobalDataStore.DataItemManager.LastFinishedTrip;

                if (result.Timestamp != null)
                    _lastMessageTime = result.Timestamp.Value;

                CarData = new ElectricCarModelItem();

                PropChanged(nameof(CarData));

                #region ToOverView
                GenerateTopics(result, chargeResult, tripResult);
                #endregion
            });
        }

        private void ChargerPolling()
        {
            if (Charger != null)
                Charger.RefreshState();

            if (Charger is GoeCharger goecharger && goecharger.OnRefreshed == null)
                goecharger.OnRefreshed = new Action(OnChargerRefreshed);
        }

        private void GenerateTopics(ElectricCarDataItem result, ChargeItem chargeResult, TripItem tripResult)
        {
            if (result == null)
            {
                _logger.LogError("Cannot Generate Topics, ElectricCarDataItem is null");
                return;
            }

            lock (locker)
            {
                if (LastTopicGenerateTimestamp.AddMilliseconds(500) > DateTime.Now)
                    return;

                Topics.Clear();

                var converter = new BatteryPercentageToPictureConverter();
                TopicModelItem topic = null;

                topic = new TopicModelItem
                {
                    Caption = "Avaliable Energy",
                    SubCaption = CarData.AvaliableEnergy.StringValue,
                    FillColor = (Color)converter.Convert(CarData.SoC.Value * 100, null, _borders, null),
                    FillPercent = CarData.DataItem.SoC,
                    ItemInstance = result.State == ElectricCarState.Driving ? this : null
                };
                Topics.Add(topic);

                topic = new TopicModelItem
                {
                    Caption = result.State == ElectricCarState.Charging ? $"Battery (Charging with: {CarData.ChargingPower.StringValue})" : "Battery",
                    SubCaption = CarData.BatteryTemperature + " (Max: +" + CarData.MaxChargePossible + ")",
                    ImageSource = "BlueThunder.png",
                    ItemInstance = result.State == ElectricCarState.Charging ? this : null
                };
                Topics.Add(topic);

                topic = new TopicModelItem
                {
                    Caption = "Climate",
                    SubCaption = CarData.ClimaMode,
                    ImageSource = "Temperature.png"
                };
                Topics.Add(topic);

                if (chargeResult != null)
                {
                    topic = new TopicModelItem
                    {
                        Caption = $"Last Charge ({chargeResult.Timestamp.ToShortDateString()})",
                        SubCaption = $"+{chargeResult.ChargedKWH} kWh +{chargeResult.ChargedRange} km (Power: {chargeResult.ChargingPointPower} kW)",
                        ImageSource = "BatteryChargingSymbol.png",
                        ItemInstance = chargeResult
                    };
                    Topics.Add(topic);
                }

                if (tripResult != null)
                {
                    topic = new TopicModelItem
                    {
                        Caption = $"Last Trip ({tripResult.Timestamp.ToShortDateString()})",
                        SubCaption = $"-{tripResult.DrivenKWH} kWh -> {tripResult.DrivenDistance} km ({tripResult.Consumption}kWh/100km)",
                        ImageSource = "ElectricCarDriving.png",
                        ItemInstance = tripResult
                    };
                    Topics.Add(topic);
                }

                PropChanged(nameof(Topics));

                LastTopicGenerateTimestamp = DateTime.Now;
            }
        }
    }
}