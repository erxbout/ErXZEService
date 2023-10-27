using System;
using System.Globalization;
using Xamarin.Forms;

namespace ErXZEService.Converter
{
    public class BatteryPercentageBorder
    {
        public int UpperBorder { get; set; }

        public int LowerBorder { get; set; }

        public Color Result { get; set; }
    }

    public class BatteryPercentageToPictureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal && parameter is BatteryPercentageBorder[])
            {
                var val = (decimal)value;
                var borders = (BatteryPercentageBorder[])parameter;
                
                for(int i = 0; i < borders.Length; i++)
                {
                    if (val >= borders[i].LowerBorder && val < borders[i].UpperBorder)
                        return borders[i].Result;
                }
            }

            return Color.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
