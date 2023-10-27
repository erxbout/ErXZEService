using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace ErXZEService.Controls
{
    public partial class ProgressRing : SKCanvasView
    {
        protected override void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            InvalidateMeasure();
            InvalidateSurface();

            base.OnPropertyChanging(propertyName);
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            var canvas = e.Surface.Canvas;
            canvas.Clear();

            int width = e.Info.Width;
            int height = e.Info.Height;

            var center = new SKPoint(width / 2, height / 2);
            var radius = (height - 20) / 2;

            var backgroundPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.FromHex("#2f3c4c").ToSKColor(),
                StrokeWidth = 15,
                IsAntialias = true
            };

            var colorPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = FillColor.ToSKColor(),
                IsStroke = true,
                StrokeWidth = 16,
                IsAntialias = true
            };

            var rect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);

            var percentage = 360f * Progress / 100;

            canvas.DrawArc(rect, 270 + percentage, 360, false, backgroundPaint);
            canvas.DrawArc(rect, 270, percentage, false, colorPaint);

            canvas.Save();
        }
    }
}
