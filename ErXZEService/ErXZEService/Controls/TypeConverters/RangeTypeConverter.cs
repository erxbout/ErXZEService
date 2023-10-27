using System;
using System.Globalization;
using Xamarin.Forms;

namespace ErXZEService.Controls.TypeConverters
{
    [TypeConverter(typeof(RangeTypeConverter))]
    public class Range
    {
        public float StartValue { get; set; }
        public float EndValue { get; set; }

        public float ValueDifference
        {
            get => EndValue - StartValue;
        }
    }
    
    [Xamarin.Forms.Xaml.TypeConversion(typeof(Range))]
    class RangeTypeConverter : TypeConverter
    {
        public override object ConvertFromInvariantString(string value)
        {
            if (value != null)
            {
                value = value.Trim();
                if (value.Contains(","))
                { //Xaml
                    var ranges = value.Split(',');
                    switch (ranges.Length)
                    {
                        case 2:
                            if (float.TryParse(ranges[0], NumberStyles.Number, CultureInfo.InvariantCulture, out float start)
                                && float.TryParse(ranges[1], NumberStyles.Number, CultureInfo.InvariantCulture, out float end))
                                return new Range() { StartValue = start, EndValue = end};
                            break;
                    }
                }
                else
                { //single uniform thickness
                    if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out float end))
                        return new Range() { EndValue = end };
                }
            }

            throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Range)}");
        }
    }
}
