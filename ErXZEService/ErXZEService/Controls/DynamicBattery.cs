using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Runtime.CompilerServices;

namespace ErXZEService.Controls
{
    public partial class DynamicBattery : SKCanvasView
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

            int baseRectangleWidth = 100;
            int baseRectangleHeight = 72;

            int strokeWidthBase = 10;

            int dotRectangleWidth = 18;
            int dotRectangleHeight = 36;

            //minpercent of colorfill
            int minPercent = 7;

            var canvas = e.Surface.Canvas;
            canvas.Clear();

            int width = e.Info.Width;
            int height = e.Info.Height;

            int widthFactor = width / (baseRectangleWidth + dotRectangleWidth + (strokeWidthBase / 2));
            int heightFactor = height / baseRectangleHeight;
            int sizeFactor = heightFactor > widthFactor ? widthFactor : heightFactor;

            int strokeFactor = sizeFactor > 1 ? sizeFactor / 2 : sizeFactor;
            int stroke = strokeWidthBase * strokeFactor;

            var backPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black
            };

            var colorPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = FillColor.ToSKColor()
            };

            var borderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(67, 72, 83),
                StrokeWidth = stroke,
                IsAntialias = true
            };

            var baseRect = new SKRect();
            baseRect.Location = new SKPoint(2 + (stroke / 2), (height / 2) - (baseRectangleHeight * sizeFactor / 2));
            baseRect.Size = new SKSize(baseRectangleWidth * sizeFactor, baseRectangleHeight * sizeFactor);

            var dotRect = new SKRect();
            dotRect.Location = new SKPoint(baseRect.Right, (height / 2) - (dotRectangleHeight * sizeFactor / 2));
            dotRect.Size = new SKSize(dotRectangleWidth * sizeFactor, dotRectangleHeight * sizeFactor);

            var colorRectFullWidth = baseRect.Right - baseRect.Left - stroke; //100%
            var drawingPercent = (float)(PercentFill < minPercent ? minPercent : PercentFill) / 100;

            var colorRect = new SKRect();
            colorRect.Location = new SKPoint(baseRect.Left + (stroke / 2), baseRect.Top + (stroke / 2));
            colorRect.Size = new SKSize(colorRectFullWidth * drawingPercent, baseRect.Bottom - baseRect.Top - stroke);

            var dotFillRect = new SKRect();
            dotFillRect.Location = new SKPoint(dotRect.Left - (stroke / 2), dotRect.Top + (stroke / 2));
            dotFillRect.Size = new SKSize(dotRect.Size.Width - (stroke / 2), dotRect.Size.Height - stroke);

            canvas.DrawRect(new SKRect(0, 0, width, height), backPaint);
            canvas.DrawRoundRect(baseRect, 4, 4, borderPaint);
            canvas.DrawRoundRect(dotRect, 4, 4, borderPaint);
            canvas.DrawRect(colorRect, colorPaint);
            canvas.DrawRect(dotFillRect, backPaint);
            canvas.Save();
        }
    }
}
