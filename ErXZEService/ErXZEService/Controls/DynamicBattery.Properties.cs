using Xamarin.Forms;

namespace ErXZEService.Controls
{
    public partial class DynamicBattery
    {
        public static readonly BindableProperty FillColorProperty = BindableProperty.Create("FillColor", typeof(Color), typeof(DynamicBattery), Color.Green);

        public Color FillColor
        {
            get => (Color)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        public static readonly BindableProperty PercentFillProperty = BindableProperty.Create(nameof(PercentFill), typeof(int), typeof(DynamicBattery), 7);

        public int PercentFill
        {
            get => (int)GetValue(PercentFillProperty);
            set => SetValue(PercentFillProperty, value);
        }
    }
}
