using ErXZEService.Models;
using ErXZEService.ViewModelItems;
using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ErXZEService.ViewModels
{
    public class TripItemViewModel : BaseViewModel
    {        
        public TripModelItem TripModel { get; private set; }

        public TripItemViewModel(TripModelItem c)
        {
            TripModel = c;
        }
    }
}
