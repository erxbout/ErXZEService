using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Runtime.CompilerServices;

namespace ErXZEService.Controls.Gauges
{
    public partial class ColorRangeGauge : SKCanvasView
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

            SKPaint backPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black
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


            //// Draw the range of values

            //var rangeStartAngle = AmountToAngle(HighlightRangeStartValue);
            //var rangeEndAngle = AmountToAngle(HighlightRangeEndValue);
            //var angleDistance = rangeEndAngle - rangeStartAngle;

            //using (SKPath path = new SKPath())
            //{
            //    path.AddArc(rect, rangeStartAngle, angleDistance);
            //    path.LineTo(center);
            //    canvas.DrawPath(path, HighlightRangePaint);
            //}

            // Draw the main gauge line/arc
            SKPaint GaugeMainLinePaintP1 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = Thickness,
                StrokeCap = SKStrokeCap.Round
            };

            var startAngle = _startAngle;
            var sweepAngle = AmountToAngle(14) - AmountToAngle(ValueRange.StartValue);

            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, startAngle, sweepAngle);
                canvas.DrawPath(path, GaugeMainLinePaintP1);
            }
            
            //Sector1.2
            SKPaint GaugeMainLinePaintP12 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Orange,
                StrokeWidth = Thickness
            };
            
            startAngle = startAngle + sweepAngle;
            sweepAngle = AmountToAngle(20) - AmountToAngle(14);
            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, startAngle, sweepAngle);
                canvas.DrawPath(path, GaugeMainLinePaintP12);
            }

            //Sector2
            SKPaint GaugeMainLinePaintP2 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Green,
                StrokeWidth = Thickness
            };
            
            startAngle = startAngle + sweepAngle;
            sweepAngle = AmountToAngle(30) - AmountToAngle(20);
            //startAngleP2 = startAngle + sweepAngle;
            //sweepAngle = startAngleP2 - AmountToAngle(28);
            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, startAngle, sweepAngle);
                canvas.DrawPath(path, GaugeMainLinePaintP2);
            }

            //Sector3
            SKPaint GaugeMainLinePaintP3 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Orange,
                StrokeWidth = Thickness
            };
            
            startAngle = startAngle + sweepAngle;
            sweepAngle = AmountToAngle(36) - AmountToAngle(30);
            //sweepAngle = startAngleP3 - AmountToAngle(34);
            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, startAngle, sweepAngle);
                canvas.DrawPath(path, GaugeMainLinePaintP3);
            }

            //Sector 4

            SKPaint GaugeMainLinePaintP4 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = Thickness,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Miter
            };
            
            startAngle = startAngle + sweepAngle;
            sweepAngle = AmountToAngle(ValueRange.EndValue) - AmountToAngle(36);
            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, startAngle, sweepAngle);
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
            canvas.Restore();
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
