using ErXZEService.ViewModelItems;
using Microcharts;
using SkiaSharp;
using System.Collections.Generic;

namespace ErXZEService.ViewModels
{
    public class ChargeItemViewModel : BaseViewModel
    {
        public bool ShowChargePowerChart { get; set; } = true;

        public Chart ChargePowerChart { get; set; }

        public ChargeModelItem ChargeModel { get; private set; }

        public ChargeItemViewModel(ChargeModelItem c)
        {
            ChargeModel = c;
            var entries = new List<ChartEntry>();
            int breakEvenPoints = -2;
            foreach (var dataPoint in c.ChargePoints)
            {
                if (dataPoint.ChargingPointPower > dataPoint.MaxChargingPower || entries.Count < 7)
                {
                    var ent = new ChartEntry((float)dataPoint.ChargingPower) { Color = SKColor.Parse("#3EC2E0") };

                    if (entries.Count == 0 || entries.Count % 5 == 0)
                        ent.ValueLabel = dataPoint.ChargingPower.ToString();

                    if (dataPoint.ChargingPointPower > dataPoint.MaxChargingPower)
                        breakEvenPoints++;

                    if ((dataPoint.ChargingPointPower > dataPoint.MaxChargingPower && breakEvenPoints == -1) || breakEvenPoints % 8 == 0 || breakEvenPoints == -2)
                    {
                        if (breakEvenPoints == -2)
                            breakEvenPoints++;
                    }

                    ent.Label = dataPoint.SoC + "%";
                    entries.Add(ent);
                }
            }

            if (entries.Count <= 7)
                ShowChargePowerChart = false;
            else
                ChargePowerChart = new LineChart { Entries = entries, LineAreaAlpha = 0, BackgroundColor = SKColor.Empty, LabelTextSize = 12 };

            PropChanged(nameof(ChargePowerChart));
            PropChanged(nameof(ShowChargePowerChart));
        }
    }
}
