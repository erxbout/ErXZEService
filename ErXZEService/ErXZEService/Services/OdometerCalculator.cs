using ErXZEService.Models;
using ErXZEService.Services.Log;
using ErXZEService.Services.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ErXZEService.Services
{
    public class OdometerCalculator
    {
        public static TripBReset DefaultTripBReset = new TripBReset { Odometer = 3234, CalculatedOdometer = 3234.2m, Timestamp = new DateTime(2019, 03, 22) };
        private readonly ILogger _logger;

        public TripBReset LastTripBReset => TripBResets.LastOrDefault();
        public long LastTripBResetOdometer => LastTripBReset == null ? 0 : LastTripBReset.Odometer;

        private List<TripBReset> TripBResets { get; set; }

        public OdometerCalculator(ILogger logger)
        {
            TripBResets = new List<TripBReset>() { DefaultTripBReset };
            _logger = logger;
        }

        public void Restore(ISession session)
        {
            SetTripBResets(session.SelectMany<TripBReset>());
        }

        public void Save(ISession session)
        {
            session.Save(TripBResets);
        }

        public void SetTripBResets(List<TripBReset> resets, [CallerMemberName] string name = null)
        {
            _logger.LogInformation("tripb resets overwritten by:" + name);
            TripBResets = resets;
        }

        public void AddTripBReset(TripBReset reset)
        {
            var checkItem = TripBResets.FirstOrDefault(x => x.CalculatedOdometer > reset.CalculatedOdometer);

            if (checkItem != null || reset.Timestamp <= TripBResets.Last().Timestamp)
            {
                _logger.LogInformation($"add tripb reset denied ({reset.Odometer}@{reset.Timestamp}) " +
                             $"because there are more recent ones: {checkItem?.Odometer}@{checkItem?.Timestamp}");

                return;
            }

            TripBResets.Add(reset);
        }

        public decimal CalculateExactFromTripBReset(decimal? tripBDistance, DateTime timestamp, [CallerFilePath] string caller = "")
        {
            var affectingTripBReset = TripBResets.LastOrDefault(x => x.Timestamp <= timestamp);

            if (affectingTripBReset == null || timestamp == DateTime.MinValue)
            {
                _logger.LogError($"no trip b reset found for distance:{tripBDistance}@{timestamp.ToShortDateString()} {timestamp.ToShortTimeString()} caller:{caller}");
                return 0;
            }

            return affectingTripBReset.CalculatedOdometer + tripBDistance.GetValueOrDefault(0m) - affectingTripBReset.TripB_Distance;
        }

        public long CalculateFromTripBReset(decimal? tripBDistance, DateTime timestamp, [CallerFilePath] string caller = "")
        {
            return (long) Math.Round(CalculateExactFromTripBReset(tripBDistance, timestamp, caller), MidpointRounding.AwayFromZero);
        }
    }
}
