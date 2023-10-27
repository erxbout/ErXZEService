using System;
using System.Globalization;
using Xamarin.Forms;

namespace ErXZEService.Controls.TypeConverters
{
    /// <summary>
    /// This is a workaround for decimal entry problems..
    /// source https://github.com/xamarin/Xamarin.Forms/issues/6579#issuecomment-621874875
    /// </summary>
    internal class DecimalValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }
            else
            { 
                return ForceDecimalUniversal(
                    (value.ToString().EndsWith(".") || value.ToString().EndsWith(","))
                    ? value.ToString() + "0"
                    : value.ToString());
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, culture);

        /// <summary>
        /// Forces conversion from object to double
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static decimal ForceDecimalUniversal(object o)
        {
            if (o == null)
            {
                return 0;
            }
            var culture = o.ToString().Contains(",") ? CultureInfo.CreateSpecificCulture("de-DE") : CultureInfo.CreateSpecificCulture("en-US");

            return !decimal.TryParse(o.ToString(), NumberStyles.Any, culture, out decimal result) ? 0 : result;
        }
    }
}
