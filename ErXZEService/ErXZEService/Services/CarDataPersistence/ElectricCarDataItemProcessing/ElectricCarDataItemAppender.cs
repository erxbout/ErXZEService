using ErXZEService.Helper;
using ErXZEService.Models;
using ErXZEService.Services.Events;
using ErXZEService.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;

namespace ErXZEService.Services.CarDataPersistence.ElectricCarDataItemProcessing
{
    public class ElectricCarDataItemAppender
    {
        private IEventService _eventService;

        public Action<Entity> OnSave { get; set; }

        public Action<ElectricCarState> OnCarStateChanged { get; set; }

        public ChargeItem CurrentCharge { get; private set; } = new ChargeItem();
        public TripItem CurrentTrip { get; private set; } = new TripItem();

        public ChargeItem LastFinishedCharge => ChargeItems.LastOrDefault();
        public TripItem LastFinishedTrip => TripItems.LastOrDefault();

        public List<ElectricCarDataItem> DataItems { get; private set; } = new List<ElectricCarDataItem>();
        public List<ChargeItem> ChargeItems { get; set; } = new List<ChargeItem>();
        public List<TripItem> TripItems { get; set; } = new List<TripItem>();

        public ElectricCarDataItem CurrentDataItem { get; set; } = ElectricCarDataItem.GetEmptyElectricCarDataItem();

        public DateTime CurrentDate => CurrentDataItem.Timestamp.Value != DateTime.MinValue ? CurrentDataItem.Timestamp.Value : DateTime.Now;

        /// <summary>
        /// Used for naming trips
        /// </summary>
        public int CurrentTripItemCount { get; set; }

        public ElectricCarDataItemAppender()
        {
            _eventService = IoC.Resolve<IEventService>();
        }

        public void SaveCurrentDataItem()
        {
            OnSave?.Invoke(CurrentDataItem);
        }

        public void StoreCurrentTripAndCharge()
        {
            Preferences.Set("CurrentCharge", JsonConvert.SerializeObject(CurrentCharge));
            Preferences.Set("CurrentTrip", JsonConvert.SerializeObject(CurrentTrip));
        }

        public void RestoreCurrentTripAndCharge()
        {
            var currentChargeJson = Preferences.Get("CurrentCharge", string.Empty);
            var currentTripJson = Preferences.Get("CurrentTrip", string.Empty);

            if (!string.IsNullOrEmpty(currentChargeJson))
                CurrentCharge = JsonConvert.DeserializeObject<ChargeItem>(currentChargeJson);

            if (!string.IsNullOrEmpty(currentTripJson))
                CurrentTrip = JsonConvert.DeserializeObject<TripItem>(currentTripJson);
        }

        public void SetCurrentDataItem(ElectricCarDataItem dataItem)
        {
            if (DataItems.Count > 0)
                return;

            DataItems.Add(dataItem);
            CurrentDataItem = dataItem;
        }

        public void MatchItemCaptions(List<ChargeItem> charges)
        {
            ChargeItems.ForEach(item =>
            {
                var charge = charges.FirstOrDefault(x =>
                    x.Timestamp.IsCloseEnough(item.Timestamp, 2) &&
                    x.PilotAmpere == item.PilotAmpere &&
                    x.Caption != item.Caption);

                if (charge != null)
                {
                    item.UserCaption = charge.UserCaption;
                    item.ChargedByBox = charge.ChargedByBox;
                    item.ChargePointLink = charge.ChargePointLink;
                    item.ChargePointId = charge.ChargePointId;
                    item.Cost = charge.Cost;
                }
            });
        }

        public void MatchItemCaptions(List<TripItem> trips)
        {
            TripItems.ForEach(item =>
            {
                var trip = trips.FirstOrDefault(x =>
                    x.Timestamp.IsCloseEnough(item.Timestamp, 2) &&
                    x.DrivenDistance == item.DrivenDistance &&
                    x.DrivenKWH == item.DrivenKWH &&
                    x.Caption != item.Caption);

                if (trip != null)
                    item.Caption = trip.Caption;
            });
        }

        public void Append(ElectricCarDataItem dataItem, bool isImporting = false)
        {
            if (dataItem == null)
            {
                return;
            }

            var stateChangedToParked =
                dataItem.State != CurrentDataItem.State
                && dataItem.State == ElectricCarState.Parked
                && !isImporting;

            if (dataItem.State != CurrentDataItem.State
                && dataItem.State != ElectricCarState.none
                && CurrentDataItem.State != ElectricCarState.none)
            {
                if (!isImporting)
                {
                    OnCarStateChanged?.Invoke(dataItem.State);
                }
                else
                {
                    _eventService.PushCarStateChanged(dataItem.State, dataItem.Timestamp);
                }
            }

            AppendTripBReset(dataItem);

            Profiler.MeasureCall(() => CurrentDataItem.Merge(dataItem), nameof(CurrentDataItem.Merge));
            DataItems.Add(dataItem);

            //Speed up appending if non critical information is available
            if (!dataItem.IsFullySpecified)
                return;

            if (!isImporting)
                AppendCCMode(dataItem);

            if (dataItem.ChargedKWH > 0 && dataItem.ChargedRange > 0 && dataItem.PilotAmpere == null)
                dataItem.PilotAmpere = 0;

            AppendChargeMeasurePoint(dataItem);

            if (CurrentDataItem.State == ElectricCarState.Driving)
                AppendTripMeasurePoint(dataItem);

            FinishCharge(dataItem, stateChangedToParked, isImporting);
            FinishTrip(dataItem, stateChangedToParked, isImporting);

            if (stateChangedToParked)
            {
                //charge
                CurrentDataItem.PilotAmpere = 0;
                CurrentDataItem.TimeToFull = 0;
                CurrentDataItem.ChargedKWH = null;
                CurrentDataItem.ChargedRange = null;

                //trip (maybe remove this from database entry)
                CurrentDataItem.EngineRPM = 0;
                CurrentDataItem.Speed = 0;
                CurrentDataItem.Consumption = 0;

                CurrentDataItem.MaxSpeed = null;
                CurrentDataItem.DrivenKWH = null;
                CurrentDataItem.DrivenDistance = null;
            }

            if (!isImporting)
            {
                SaveCurrentDataItem();
                StoreCurrentTripAndCharge();
            }
        }

        private void AppendTripBReset(in ElectricCarDataItem dataItem)
        {
            if (CurrentDataItem.TripB_Distance == null || dataItem.TripB_Distance == null)
                return;

            if (dataItem.TripB_Distance < CurrentDataItem.TripB_Distance && dataItem.TripB_Distance < 20)
            {
                var timestamp = dataItem.Timestamp.HasValue ? dataItem.Timestamp.Value : CurrentDataItem.Timestamp.Value;

                var newReset = new TripBReset()
                {
                    Odometer = GlobalDataStore.OdoCalculator.CalculateFromTripBReset(CurrentDataItem.TripB_Distance, timestamp),
                    CalculatedOdometer = GlobalDataStore.OdoCalculator.CalculateExactFromTripBReset(CurrentDataItem.TripB_Distance, timestamp),
                    Timestamp = timestamp.Date
                };

                GlobalDataStore.OdoCalculator.AddTripBReset(newReset);
            }
        }

        private void AppendCCMode(in ElectricCarDataItem dataItem)
        {

        }

        private void AppendChargeMeasurePoint(in ElectricCarDataItem dataItem)
        {
            if (dataItem.TripB_Distance != null)
            {
                CurrentCharge.TripB_Distance = dataItem.TripB_Distance.Value;
                CurrentCharge.TripB_AverageConsumption = dataItem.TripB_AverageConsumption.Value;
            }
            else
            {
                CurrentCharge.TripB_Distance = CurrentDataItem.TripB_Distance.Value;
                CurrentCharge.TripB_AverageConsumption = CurrentDataItem.TripB_AverageConsumption.Value;
            }

            if (dataItem.PilotAmpere == null)
                return;

            if (dataItem.SoC < CurrentCharge.StartSoC)
            {
                CurrentCharge = new ChargeItem();
                CurrentCharge.StartSoC = dataItem.SoC;
            }

            ChargePoint lastChargePoint = null;
            short ambientTemperature = 0;

            if (CurrentCharge.ChargePoints.Any())
                lastChargePoint = CurrentCharge.ChargePoints.Last();

            if (CurrentCharge.PilotAmpere < dataItem.PilotAmpere)
                CurrentCharge.PilotAmpere = dataItem.PilotAmpere.Value;

            if (CurrentCharge.Timestamp == DateTime.MinValue && dataItem.PilotAmpere > 0)
            {
                CurrentCharge.Timestamp = CurrentDate;
                CurrentCharge.StartSoC = dataItem.SoC;
                CurrentCharge.EstimatedRangeStart = dataItem.EstimatedRange;
                CurrentCharge.AvaliableEnergyStart = dataItem.AvaliableEnergy;
            }

            if (dataItem.AmbientTemperature != null)
                ambientTemperature = dataItem.AmbientTemperature.Value;
            else if (lastChargePoint != null)
                ambientTemperature = lastChargePoint.AmbientTemperature;

            CurrentCharge.ChargePoints.Add(new ChargePoint()
            {
                AvaliableEnergy = dataItem.AvaliableEnergy,
                EstimatedRange = dataItem.EstimatedRange,
                PilotAmpere = dataItem.PilotAmpere.Value,
                ChargingPointPower = dataItem.MaxPowerThreePhase,
                MaxChargingPower = dataItem.MaxChargePower.GetValueOrDefault(),
                SoC = dataItem.SoC,
                AmbientTemperature = ambientTemperature,
                BatteryTemperature = dataItem.BatteryTemperature,
                Timestamp = DateTime.Now
            });
        }

        private void AppendTripMeasurePoint(in ElectricCarDataItem dataItem)
        {
            if (dataItem.DrivenDistance != null || dataItem.DrivenKWH != null)
                return;

            if (dataItem.TripB_AverageConsumption == null || dataItem.TripB_AverageSpeed == null || dataItem.TripB_Distance == null || dataItem.TripB_UsedBattery == null)
                return;

            var trippoint = new TripPoint()
            {
                AvaliableEnergy = dataItem.AvaliableEnergy,
                SoC = dataItem.SoC,
                BatteryTemperature = dataItem.BatteryTemperature,
                TripB_Distance = dataItem.TripB_Distance.Value,
                TripB_AverageConsumption = dataItem.TripB_AverageConsumption.Value,
                TripB_UsedBattery = dataItem.TripB_UsedBattery.Value,
                TripB_AverageSpeed = dataItem.TripB_AverageSpeed.Value,

                Speed = dataItem.Speed.GetValueOrDefault(0)
            };

            CurrentTrip.TripB_Distance = dataItem.TripB_Distance;
            CurrentTrip.TripPoints.Add(trippoint);
        }

        private void FinishCharge(in ElectricCarDataItem dataItem, in bool stateChangedToParked, in bool isImporting)
        {
            if (dataItem.ChargedKWH == null && !stateChangedToParked)
                return;

            var lastCharge = ChargeItems.LastOrDefault();

            // ensure to only take the trip once
            if (lastCharge != null
                && lastCharge.ChargedKWH == dataItem.ChargedKWH
                && lastCharge.ChargedRange == dataItem.ChargedRange)
                return;

            CurrentCharge.EndSoC = dataItem.SoC;
            CurrentCharge.EstimatedRangeEnd = dataItem.EstimatedRange;
            CurrentCharge.AvaliableEnergyEnd = dataItem.AvaliableEnergy;
            CurrentCharge.ChargedKWH = dataItem.ChargedKWH != null ? dataItem.ChargedKWH.Value : CurrentCharge.CurrentChargedEnergy;
            CurrentCharge.ChargedRange = dataItem.ChargedRange != null ? dataItem.ChargedRange.Value : CurrentCharge.CurrentChargedRange;

            if (CurrentCharge.ChargePoints.Any())
            {
                CurrentCharge.ChargePointPower = CurrentCharge.ChargePoints.Max(x => x.ChargingPointPower);
            }

            if (CurrentCharge.Timestamp == DateTime.MinValue)
                CurrentCharge.Timestamp = CurrentDate;

            // skip empty shit for now..
            if (CurrentCharge.ChargedKWH == 0 && CurrentCharge.ChargedRange == 0)
                return;

            if (CurrentCharge.TripB_Distance == null)
                CurrentCharge.TripB_Distance = CurrentDataItem.TripB_Distance;
            CurrentCharge.Odometer = CurrentCharge.CalculatedOdometer;

            if (!isImporting)
                OnSave?.Invoke(CurrentCharge);

            ChargeItems.Add(CurrentCharge);
            GlobalDataStore.AddedTopicModelItem(CurrentCharge);
            CurrentCharge = new ChargeItem();
        }

        private void FinishTrip(in ElectricCarDataItem dataItem, in bool stateChangedToParked, in bool isImporting)
        {
            if ((dataItem.DrivenDistance == null || dataItem.DrivenKWH == null) && !stateChangedToParked)
                return;

            if (CurrentTrip.TripPoints.Count == 0 && stateChangedToParked)
                return;

            if (LastFinishedTrip != null
                && LastFinishedTrip.DrivenKWH == dataItem.DrivenKWH
                && LastFinishedTrip.DrivenDistance == dataItem.DrivenDistance)
                return;

            CurrentTrip.ApplyStartAndEnd();

            CurrentTrip.DrivenKWH = dataItem.DrivenKWH != null ? dataItem.DrivenKWH.Value : CurrentTrip.CurrentUsedEnergy;
            CurrentTrip.DrivenDistance = dataItem.DrivenDistance != null ? dataItem.DrivenDistance.Value : CurrentTrip.CurrentTripDistance;
            CurrentTrip.MaxSpeed = dataItem.MaxSpeed != null ? dataItem.MaxSpeed.Value : CurrentTrip.CurrentMaxSpeed;

            CurrentTrip.Timestamp = CurrentDate;
            CurrentTrip.Odometer = GlobalDataStore.OdoCalculator.CalculateFromTripBReset(CurrentTrip.TripB_Distance, CurrentTrip.Timestamp);

            // skip empty shit for now..
            if (CurrentTrip.DrivenDistance == 0 || CurrentTrip.DrivenKWH == 0)
                return;

            if (CurrentTrip.DrivenDistance < 0.3m && CurrentTrip.DrivenDistance > -0.1m)
                CurrentTrip.Caption = "Moved " + CurrentTripItemCount;
            else
                CurrentTrip.Caption = "Trip " + CurrentTripItemCount;

            LastFinishedCharge?.Trips.Add(CurrentTrip);

            TripItems.Add(CurrentTrip);
            GlobalDataStore.AddedTopicModelItem(CurrentTrip);

            if (!isImporting)
                OnSave?.Invoke(CurrentTrip);

            CurrentTripItemCount++;
            CurrentTrip = new TripItem();
        }
    }
}
