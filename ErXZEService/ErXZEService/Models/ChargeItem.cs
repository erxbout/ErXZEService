using ErXZEService.Services;
using ErXZEService.Services.Log;
using ErXZEService.Services.SQL;
using ErXZEService.Services.SQL.Attributes;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ErXZEService.Models
{
    public class DifferenceItem<T>
    {
        public T First { get; set; }
        public T Last { get; set; }
    }

    [Table(nameof(ChargeItem))]
    public class ChargeItem : Entity, ITopicModelItem
    {
        public long ChargeItemGroupId { get; set; }

        public DateTime Timestamp { get; set; }

		[Ignore]
		public string Caption => $"{UserCaption} ({StartSoC}-{EndSoC}%)";

		public string UserCaption
        {
			get => _userCaption;
			set
            {
				if (_userCaption != value)
				{
					_userCaption = value;
					Changed = true;
            }
			}
		}

		public string Description
		{
			get => _description;
            set
            {
				if (_description != value)
				{
					_description = value;
                    Changed = true;
				}
            }
        }

        public string ChargePointId { get; set; }

		public string ChargePointLink
		{
			get => _chargePointLink;
			set
			{
				if (_chargePointLink != value)
				{
					_chargePointLink = value;
					Changed = true;
				}
			}
		}

        /// <summary>
        /// Defines the energy amount in kwh charged on the box side
        /// </summary>
		public decimal ChargedByBox
		{
			get => _chargedByBox;
			set
			{
				if (_chargedByBox != value)
				{
					_chargedByBox = value;
					Changed = true;
				}
			}
		}

        /// <summary>
        /// Payed for the charge
        /// </summary>
		public decimal Cost
		{
			get => _cost;
			set
			{
				if (_cost != value)
				{
					_cost = value;
					Changed = true;
				}
			}
		}

        [Ignore]
        public string SubCaption { get => $"+{ChargedKWH} kWh +{ChargedRange} km (Power: {ChargingPointPower} kW)"; }

        [Ignore]
        public string MonthCaption { get => $"{Timestamp.Day}.{Timestamp.Month}"; }

        [Ignore]
        public string ChargeCaption
        {
            get
            {
				if (UserCaption == null)
                {
					UserCaption = "#Ort";
                }

                return Caption;
            }
			set
			{
				UserCaption = value;
			}
        }

        [Ignore]
        [OneToMany(nameof(ChargePoint.ChargeItemId))]
        public List<ChargePoint> ChargePoints { get; set; } = new List<ChargePoint>();

        [Ignore]
        public decimal CurrentChargedEnergy
        {
            get
            {
                var dif = GetDifferenceItem(x => x.AvaliableEnergy);
                return dif.Last - dif.First;
            }
        }

        [Ignore]
        public short CurrentChargedRange
        {
            get
            {
                var dif = GetDifferenceItem(x => x.EstimatedRange);
                return (short)(dif.Last - dif.First);
            }
        }

        [Ignore]
        public string CurrentChargeTime
        {
            get
            {
                var start = ChargePoints.FirstOrDefault();
                var timestamp = start != null ? start.Timestamp : DateTime.Now;
                var dif = DateTime.Now - timestamp;

                return $"Chargetime: {dif.ToString(@"hh\:mm\:ss")}";
            }
        }

        [Ignore]
        public string Charged
        {
            get
            {
                var dif = GetDifferenceItem(x => x.AvaliableEnergy);

                return $"Charged: {dif.Last - dif.First}kWh ({dif.First}kWh-{dif.Last}kWh)";
            }
        }

        [Ignore]
        public string ChargeRate
        {
            get
            {
                var lastItem = ChargePoints.LastOrDefault();

                var rate = lastItem != null && TripB_AverageConsumption > 0 ? Math.Round(lastItem.ChargingPower * 100 / TripB_AverageConsumption, 1) : 0;

                return $"ChargeRate: {rate}km/h";
            }
        }

        [Ignore]
        public string SocCharged
        {
            get
            {
                var dif = GetDifferenceItem(x => x.SoC);

                return $"SoC: +{dif.Last - dif.First}% ({dif.First}%-{dif.Last}%)";
            }
        }

        [Ignore]
        public string RangeCharged
        {
            get
            {
                var dif = GetDifferenceItem(x => x.EstimatedRange);

                return $"Range: {dif.Last - dif.First}km ({dif.First}km-{dif.Last}km)";
            }
        }

        public DifferenceItem<T> GetDifferenceItem<T>(Func<ChargePoint, T> func)
        {
            var firstPoint = ChargePoints.FirstOrDefault();
            var lastPoint = ChargePoints.LastOrDefault();

            return new DifferenceItem<T>()
            {
                First = firstPoint != null ? func(firstPoint) : default,
                Last = lastPoint != null ? func(lastPoint) : default
            };
        }

        private ChargeItemGroup _chargeItemGroup;
		private string _userCaption;
		private string _description;
		private string _chargePointLink;
		private decimal _chargedByBox;
		private decimal _cost;

        [Ignore]
        public ChargeItemGroup ChargeItemGroup
        {
            get
            {
                if (ChargeItemGroupId > 0)
                {
                    if (_chargeItemGroup == null)
                    {
                        using (var session = new SQLiteSession(IoC.Resolve<ILogger>()))
                            _chargeItemGroup = session.SelectSingleOrDefault<ChargeItemGroup>(x => x.Id == ChargeItemGroupId);
                    }
                }

                return _chargeItemGroup;
            }
        }

        [Ignore]
        [OneToMany(nameof(TripItem.ChargeItemId))]
        public List<TripItem> Trips { get; set; } = new List<TripItem>();

        public bool IsThreePhase { get; set; } = true;

        [Ignore]
        public bool IsCurrentCharge { get; set; }

        public decimal ChargePointPower { get; set; }

        #region Distance
        public decimal? TripB_Distance { get; set; }
        public decimal TripB_AverageConsumption { get; set; }

        public long Odometer { get; set; }

        public long CalculatedOdometer
        {
            get
            {
                return GlobalDataStore.OdoCalculator.CalculateFromTripBReset(TripB_Distance, Timestamp);
            }
        }
        #endregion

        [Ignore]
        public decimal ChargingPointPower
        {
            get
            {
                decimal max = 0;

                foreach (var chargepoint in ChargePoints)
                    if (chargepoint.ChargingPointPower > max)
                        max = chargepoint.ChargingPointPower;

                max = Math.Round(max, 1);

                if (ChargePointPower < max)
                    ChargePointPower = max;

                return max;
            }
        }

        #region Estimation
        public short EstimatedRangeStart { get; set; }
        public short EstimatedRangeEnd { get; set; }
        #endregion

        #region Charging
        public byte PilotAmpere { get; set; }

        public decimal ChargedKWH { get; set; }

        public short ChargedRange { get; set; }
        #endregion

        public short StartSoC { get; set; }
        public short EndSoC { get; set; }

        #region Battery
        public decimal AvaliableEnergyStart { get; set; }
        public decimal AvaliableEnergyEnd { get; set; }
        #endregion
    }
}
