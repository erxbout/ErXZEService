using Xamarin.Forms;

namespace ErXZEService.Controls
{
    public partial class ProgressRing
    {
        public static readonly BindableProperty FillColorProperty = BindableProperty.Create("FillColor", typeof(Color), typeof(ProgressRing), Color.Green);

        public Color FillColor
        {
            get => (Color)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(int), typeof(ProgressRing), 0);

        public int Progress
        {
            get => (int)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }
    }
}
