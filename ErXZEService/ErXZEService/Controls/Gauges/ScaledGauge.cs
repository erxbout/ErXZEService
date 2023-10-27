using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;

namespace ErXZEService.Controls.Gauges
{
    public partial class ScaledGauge : SKCanvasView
    {
        public ScaledGauge()
        {
            WidthRequest = 400;
            HeightRequest = 250;
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            var canvas = e.Surface.Canvas;
            canvas.Clear();

            int width = e.Info.Width;
            int height = e.Info.Height;

            SKPaint backPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black,
            };

            canvas.DrawRect(new SKRect(0, 0, width, height), backPaint);

            canvas.Save();

            //set start of coordinates to center of canvas
            canvas.Translate(width / 2, height / 2.2f);
            canvas.Scale(Math.Min(width / 105f, height / 260f));
            SKPoint center = new SKPoint(0, 0);

            var rect = new SKRect(-100, -100, 100, 100);

            // Add a buffer for the rectangle
            rect.Inflate(-10, -10);

            SKPaint GaugePointPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = ValueColor.ToSKColor()
            };

            SKPaint HighlightRangePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = RangeColor.ToSKColor()
            };

            SKPaint GaugeMainLinePaintP4 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.ForestGreen,
                StrokeWidth = Thickness,
                StrokeCap = SKStrokeCap.Round
            };

            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, _startAngle, _endAngle);
                canvas.DrawPath(path, GaugeMainLinePaintP4);
            }

            //Draw Needle
            DrawNeedle(canvas, Value);

            //Draw Screw
            SKPaint NeedleScrewPaint = new SKPaint()
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(center, width / 60, new SKColor[]
               { new SKColor(171, 171, 171), SKColors.Black }, new float[] { 0.05f, 0.9f }, SKShaderTileMode.Mirror)
            };

            canvas.DrawCircle(center.X, center.Y, width / 60, NeedleScrewPaint);

            SKPaint paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(81, 84, 89).WithAlpha(100),
                StrokeWidth = 1f
            };

            canvas.DrawCircle(center.X, center.Y, width / 60, paint);

            // Draw the Units of Measurement Text on the display
            SKPaint textPaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Black
            };

            float textWidth = textPaint.MeasureText(DescriptionText);
            textPaint.TextSize = DescriptionFontSize;
            textPaint.Color = TextColor.ToSKColor();

            SKRect textBounds = SKRect.Empty;
            textPaint.MeasureText(DescriptionText, ref textBounds);

            float xText = -1 * textBounds.MidX;
            float yText = 120 - textBounds.Height;

            // And draw the text
            canvas.DrawText(DescriptionText, xText, yText, textPaint);
            
            // Draw the Value on the display
            var valueText = Value.ToString("F0") + UnitsText; //You can set F1 or F2 if you need float values
            float valueTextWidth = textPaint.MeasureText(valueText);
            textPaint.TextSize = ValueFontSize;
            textPaint.Color = TextColor.ToSKColor();

            textPaint.MeasureText(valueText, ref textBounds);

            xText = -1 * textBounds.MidX;
            yText = 85 - textBounds.Height;

            // And draw the text
            canvas.DrawText(valueText, xText, yText, textPaint);

            DrawScale(canvas);

            canvas.Restore();
        }

        private void DrawScale(SKCanvas canvas)
        {
            //Scale lines
            int dotsDrawn = 0;
            canvas.RotateDegrees(-_startAngle);

            var linePaint = new SKPaint()
            {
                Color = SKColors.White
            };

            var textPaint = new SKPaint()
            {
                Color = SKColors.White
            };

            for (int angle = 0; angle <= _endAngle; angle += 6)
            {
                bool biggerCircle = dotsDrawn % 5 == 0;

                var lineY = -80;

                if (biggerCircle)
                {
                    lineY = -75;
                    linePaint.StrokeWidth = Thickness / 6;

                    //draw scale text
                    //float textWidth = textPaint.MeasureText(angle.ToString());
                    //textPaint.TextSize = 20;
                    //textPaint.Color = TextColor.ToSKColor();

                    //// And draw the text
                    //canvas.DrawText(angle.ToString(), -20, -100, textPaint);
                }
                else
                {
                    linePaint.StrokeWidth = Thickness / 12;
                }

                canvas.DrawLine(0, -95, 0, lineY, linePaint);

                canvas.RotateDegrees(6);
                dotsDrawn++;
            }
        }

        float AmountToAngle(float value)
        {
            return ((value - ValueRange.StartValue) / ValueRange.ValueDifference) * _endAngle - _startAngle;
        }

        void DrawNeedle(SKCanvas canvas, float value)
        {
            //float angle = -135f + (value / (100 - 0)) * 270f;
            float angle = AmountToAngle(value);
            canvas.Save();
            canvas.RotateDegrees(angle);
            float needleWidth = 8f;
            float needleHeight = 76f;
            float x = 0f, y = 0f;

            SKPaint paint = new SKPaint
            {
                IsAntialias = true,
                Color = NeedleColor.ToSKColor()
            };

            SKPath needleRightPath = new SKPath();
            needleRightPath.MoveTo(x, y);
            needleRightPath.LineTo(x + needleWidth, y);
            needleRightPath.LineTo(x, y - needleHeight);
            needleRightPath.LineTo(x, y);
            needleRightPath.LineTo(x + needleWidth, y);


            SKPath needleLeftPath = new SKPath();
            needleLeftPath.MoveTo(x, y);
            needleLeftPath.LineTo(x - needleWidth, y);
            needleLeftPath.LineTo(x, y - needleHeight);
            needleLeftPath.LineTo(x, y);
            needleLeftPath.LineTo(x - needleWidth, y);


            canvas.DrawPath(needleRightPath, paint);
            canvas.DrawPath(needleLeftPath, paint);
            canvas.Restore();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            // Determine when to change. Basically on any of the properties that we've added that affect
            // the visualization, including the size of the control, we'll repaint
            if (propertyName == HighlightRangeEndValueProperty.PropertyName
                || propertyName == HighlightRangeStartValueProperty.PropertyName
                || propertyName == ValueProperty.PropertyName
                || propertyName == WidthProperty.PropertyName
                || propertyName == HeightProperty.PropertyName
                || propertyName == GaugeLineColorProperty.PropertyName
                || propertyName == ValueColorProperty.PropertyName
                || propertyName == RangeColorProperty.PropertyName
                || propertyName == UnitsTextProperty.PropertyName)
            {
                InvalidateSurface();
            }
        }
    }
}
