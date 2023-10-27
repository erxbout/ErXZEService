using ErXZEService.Controls.TypeConverters;
using Xamarin.Forms;

namespace ErXZEService.Controls.Gauges
{
    public partial class ScaledGauge
    {
        private float _startAngle = 135f;
        private float _endAngle = 270f;
      
        public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(nameof(Thickness), typeof(int), typeof(ColorRangeGauge), 10);

        public int Thickness
        {
            get { return (int)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }
        
        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(float), typeof(ColorRangeGauge));

        public float Value
        {
            get { return (float)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        
        public static readonly BindableProperty ValueRangeProperty = BindableProperty.Create(nameof(ValueRange), typeof(Range), typeof(ColorRangeGauge), new Range() { EndValue = 100 });

        public Range ValueRange
        {
            get { return (Range)GetValue(ValueRangeProperty); }
            set { SetValue(ValueRangeProperty, value); }
        }

        public static readonly BindableProperty HighlightRangeStartValueProperty = BindableProperty.Create("HighlightRangeStartValue", typeof(float), typeof(ColorRangeGauge), 70.0f);

        public float HighlightRangeStartValue
        {
            get { return (float)GetValue(HighlightRangeStartValueProperty); }
            set { SetValue(HighlightRangeStartValueProperty, value); }
        }

        public static readonly BindableProperty HighlightRangeEndValueProperty = BindableProperty.Create("HighlightRangeEndValue", typeof(float), typeof(ColorRangeGauge), 100.0f);

        public float HighlightRangeEndValue
        {
            get { return (float)GetValue(HighlightRangeEndValueProperty); }
            set { SetValue(HighlightRangeEndValueProperty, value); }
        }

        // Properties for the Colors
        public static readonly BindableProperty GaugeLineColorProperty = BindableProperty.Create("GaugeLineColor", typeof(Color), typeof(ColorRangeGauge), Color.FromHex("#70CBE6"));

        public Color GaugeLineColor
        {
            get { return (Color)GetValue(GaugeLineColorProperty); }
            set { SetValue(GaugeLineColorProperty, value); }
        }

        public static readonly BindableProperty ValueColorProperty = BindableProperty.Create("ValueColor", typeof(Color), typeof(ColorRangeGauge), Color.FromHex("FF9A52"));

        public Color ValueColor
        {
            get { return (Color)GetValue(ValueColorProperty); }
            set { SetValue(ValueColorProperty, value); }
        }

        public static readonly BindableProperty RangeColorProperty = BindableProperty.Create("RangeColor", typeof(Color), typeof(ColorRangeGauge), Color.FromHex("#E6F4F7"));

        public Color RangeColor
        {
            get { return (Color)GetValue(RangeColorProperty); }
            set { SetValue(RangeColorProperty, value); }
        }

        public static readonly BindableProperty NeedleColorProperty = BindableProperty.Create("NeedleColor", typeof(Color), typeof(ColorRangeGauge), Color.FromRgb(252, 18, 30));

        public Color NeedleColor
        {
            get { return (Color)GetValue(NeedleColorProperty); }
            set { SetValue(NeedleColorProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(ColorRangeGauge), Color.Black);

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        // Properties for the Units

        public static readonly BindableProperty UnitsTextProperty = BindableProperty.Create("UnitsText", typeof(string), typeof(ColorRangeGauge), "");

        public string UnitsText
        {
            get { return (string)GetValue(UnitsTextProperty); }
            set { SetValue(UnitsTextProperty, value); }
        }

        public static readonly BindableProperty DescriptionFontSizeProperty = BindableProperty.Create(nameof(DescriptionFontSize), typeof(float), typeof(ColorRangeGauge), 14f);

        public float DescriptionFontSize
        {
            get { return (float)GetValue(DescriptionFontSizeProperty); }
            set { SetValue(DescriptionFontSizeProperty, value); }
        }

        public static readonly BindableProperty DescriptionTextProperty = BindableProperty.Create(nameof(DescriptionText), typeof(string), typeof(ColorRangeGauge), "");

        public string DescriptionText
        {
            get { return (string)GetValue(DescriptionTextProperty); }
            set { SetValue(DescriptionTextProperty, value); }
        }

        public static readonly BindableProperty ValueFontSizeProperty = BindableProperty.Create("ValueFontSize", typeof(float), typeof(ColorRangeGauge), 33f);

        public float ValueFontSize
        {
            get { return (float)GetValue(ValueFontSizeProperty); }
            set { SetValue(ValueFontSizeProperty, value); }
        }
    }
}
