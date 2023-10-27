using System;

namespace ErXZEService.Services
{
    public static class RemainingChargeTimeCalculator
    {
        public static TimeSpan TryCalculateTimeSpan(decimal targetInKwh, decimal currentKwh, decimal chargePowerInKw, bool includeNegative = false)
        {
            try
            {
                return CalculateTimeSpan(targetInKwh, currentKwh, chargePowerInKw, includeNegative);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        public static TimeSpan CalculateTimeSpan(decimal targetInKwh, decimal currentKwh, decimal chargePowerInKw, bool includeNegative = false)
        {
            if (chargePowerInKw == 0)
                throw new ArgumentException("ChargePower can not be 0");

            if (targetInKwh == currentKwh || (targetInKwh < currentKwh && !includeNegative))
                return TimeSpan.Zero;

            var dif = targetInKwh - currentKwh;
            var hours = chargePowerInKw > 0 ? dif / chargePowerInKw : 0;
            var minutes = Math.Round((hours - Math.Floor(hours)) * 60);

            var result = new TimeSpan((int)Math.Floor(hours), (int)minutes, 0);

            return result;
        }

        /// <summary>
        /// Calculates the progress in 100 percent
        /// </summary>
        public static decimal CalculateProgress(decimal targetInKwh, decimal currentKwh, int decimals = 2)
        {
            if (currentKwh >= targetInKwh)
                return 1;

            if (targetInKwh == 0)
                return 0;

            return Math.Round(currentKwh / targetInKwh * 100m, decimals);
        }
    }
}
