using ErXBoutCode.MVVM;
using ErXZEService.Converter;
using ErXZEService.Models;
using ErXZEService.Services;
using ErXZEService.Services.Log;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace ErXZEService.ViewModels
{
    public partial class OverviewViewModel : BaseViewModel
    {
        #region Resources
        private BatteryPercentageBorder[] _borders =
        {
            new BatteryPercentageBorder()
            {
                LowerBorder = 0,
                UpperBorder = 20,
                Result = Color.Red
            },
            new BatteryPercentageBorder()
            {
                LowerBorder = 20,
                UpperBorder = 45,
                Result = Color.FromHex("#ff6a00") //orange
            },
            new BatteryPercentageBorder()
            {
                LowerBorder = 45,
                UpperBorder = 101,
                Result = Color.Green
            }
        };

        private StateViewItem[] _connectionStateViews =
        {
            new StateViewItem()
            {
                ImageSource = "fail.png",
                State = "Offline"
            },

            new StateViewItem()
            {
                ImageSource = "Attention.png",
                State = "Waiting (WiFi)"
            },

            new StateViewItem()
            {
                ImageSource = "ok.png",
                State = "Online (WiFi)"
            },

            new StateViewItem()
            {
                ImageSource = "Attention.png",
                State = "Online (Data)"
            },
        };
        #endregion

        #region Properties
        private ElectricCarModelItem _carData;

        public ElectricCarModelItem CarData
        {
            get { return _carData; }
            set { _carData = value; PropChanged(); }
        }

        public ChargeItem CurrentCharge => GlobalDataStore.DataItemManager?.CurrentCharge == null ? new ChargeItem() : GlobalDataStore.DataItemManager?.CurrentCharge;

        private ConnectionType _connectionType;

        public ConnectionType ConnectionType
        {
            get { return _connectionType; }
            set
            {
                _logger.LogInformation($"setting connection type from '{_connectionType}' to : {value}");

                if (_connectionType != value)
                {
                    _connectionType = value;

                    if (!_isLoading)
                    {
                        ConnectionStateItem = _connectionStateViews[(byte)_connectionType];
                        PropChanged(nameof(ConnectionStateItem));
                    }
                }
            }
        }


        private string _additionalInfoLabelText;

        public string AdditionalInfoLabelText
        {
            get
            {
                return _additionalInfoLabelText;
            }
            set
            {
                _additionalInfoLabelText = value;
                PropChanged();
                PropChanged(nameof(AdditionalInfoLabelTextVisible));
            }
        }

        public bool AdditionalInfoLabelTextVisible => AdditionalInfoLabelText != "";

        private string _lastMessageTimeSource = "";

        private string _lastMessageTimeStr = "-";

        public string LastMessageTime
        {
            get { return _lastMessageTimeStr; }
            set { _lastMessageTimeStr = value; PropChanged(); }
        }

        public StateViewItem ConnectionStateItem { get; set; }

        private DateTime _lastMessageTimeField;

        private DateTime _lastMessageTime
        {
            set
            {
                _lastMessageTimeField = value;
                string result = _lastMessageTimeField.ToShortDateString() + " " + _lastMessageTimeField.ToShortTimeString();

                if (!string.IsNullOrEmpty(_lastMessageTimeSource))
                    result += " (" + _lastMessageTimeSource + ")";

                LastMessageTime = result;
            }
        }

        private ObservableCollection<TopicModelItem> _topics = new ObservableCollection<TopicModelItem>();

        public ObservableCollection<TopicModelItem> Topics
        {
            get { return _topics; }
            set { _topics = value; PropChanged(); }
        }

        public bool CruiseControlActive { get; set; }
        public bool ChargeTimerStarted { get; set; }

        public bool ABRPTelemetryEnabled => _configuration.UserSettings.AbrpIntegration.Enabled;

        public string ThresholdButtonText => Charger.DataItem.KwhThreshold == 0 ? $"Threshold = unset" : $"Threshold = {Charger.DataItem.KwhThreshold}kWh";

        public bool ProgressVisible => Charger.DataItem.KwhThreshold != 0;

        public string RemainingText
        {
            get
            {
                var remaining = RemainingChargeTimeCalculator.TryCalculateTimeSpan(Charger.DataItem.KwhThreshold, Charger.DataItem.CurrentChargeKwh, (decimal)CarData.ChargingPower.Value);
                return $"Remaining = {remaining:hh\\:mm}";
            }
        }

        public string ProgressText
        {
            get
            {
                var progress = RemainingChargeTimeCalculator.CalculateProgress(Charger.DataItem.KwhThreshold, Charger.DataItem.CurrentChargeKwh);

                return $"Progress: {progress}%";
            }
        }

        public double ProgressValue
        {
            get
            {
                var progress = RemainingChargeTimeCalculator.CalculateProgress(Charger.DataItem.KwhThreshold, Charger.DataItem.CurrentChargeKwh);

                return (double)progress / 100D;
            }
        }
        #endregion

        #region ActionCommands
        public ActionCommand ShowTirePressures { get; set; }

        public ActionCommand DebugChangeState { get; set; }

        public ActionCommand ReImport { get; set; }
        public ActionCommand Import { get; set; }

        public ActionCommand ChangeThreshold { get; set; }
        public ActionCommand ChangeDesiredCurrent { get; set; }
        #endregion
    }
}