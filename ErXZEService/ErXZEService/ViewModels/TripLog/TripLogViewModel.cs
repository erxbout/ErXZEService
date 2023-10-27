using ErXBoutCode.MVVM;
using ErXZEService.Models;
using ErXZEService.Services;
using Microcharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ErXZEService.ViewModels
{
    public class TripLogViewModel : BaseViewModel, IDisposable
    {
        public string TotalDistanceActualMonth { get; set; }
        public string TotalDistanceLastMonth { get; set; }
        public string AvgDistance { get; set; }

        public double Percentage { get; set; }

        public int MonthlyKilometers { get; set; } = 1354;

        private Chart _chart;

        public Chart ShowChart
        {
            get => _chart;
            set { _chart = value; PropChanged(); }
        }

        public ActionCommand NextPage { get; set; }
        public ActionCommand PreviousPage { get; set; }

        public string CurrentPageText => "Current Page: " + CurrentPage;

        private int CurrentPage { get; set; } = 1;
        private int _lastMonth;

        /// <summary>
        /// this is for storing the pageination set index for adding to the collection
        /// </summary>
        private int _currentPageinationSetIndex = 1;

        public List<TopicModelItem> TripItems { get; set; } = new List<TopicModelItem>();

        public List<TopicModelItem> PageinatedTripItems
        {
            get
            {
                var result = TripItems
                    .SkipWhile(x => x.PageinationSetIndex < CurrentPage)
                    .TakeWhile(x => x.PageinationSetIndex == CurrentPage)
                    .ToList();

                return result;
            }
        }

        public TripLogViewModel()
        {
            NextPage = new ActionCommand(() =>
            {
                if (CurrentPage < TripItems.Max(x => x.PageinationSetIndex))
                    CurrentPage++;
                PropChanged(nameof(PageinatedTripItems));
                PropChanged(nameof(CurrentPageText));
            });

            PreviousPage = new ActionCommand(() =>
            {
                if (CurrentPage > 1)
                    CurrentPage--;
                PropChanged(nameof(PageinatedTripItems));
                PropChanged(nameof(CurrentPageText));
            });

            CalculateKm();
            InitViewModel();
            Title = "Log";

            GlobalDataStore.TopicModelItemAdded += GlobalDataStore_TopicModelItemAdded;
        }

        private void GlobalDataStore_TopicModelItemAdded(ITopicModelItem item)
        {
            if (item is TripItem tripItem)
            {
                int lastIndex = TripItems.Count == 0 ? 0 : TripItems.Max(x => x.Index);
                AddTripItemToObservableCollection(ref lastIndex, tripItem);
                TripItems.OrderByDescending(x => x.Index);
                PropChanged(nameof(TripItems));
                PropChanged(nameof(PageinatedTripItems));
                CalculateKm();
            }
        }

        public void Dispose()
        {
            GlobalDataStore.TopicModelItemAdded -= GlobalDataStore_TopicModelItemAdded;
        }

        private void CalculateKm()
        {
            Task.Run(() =>
            {
                while (GlobalDataStore.DataItemManager == null)
                    Thread.Sleep(100);

                var trips = GlobalDataStore.DataItemManager.TripItems;

                var totalThisMonth = trips.Where(x => x.Timestamp.Month == DateTime.Now.Month).Sum(y => y.DrivenDistance);
                var totalLastMonth = trips.Where(x => x.Timestamp.Month == DateTime.Now.AddMonths(-1).Month).Sum(y => y.DrivenDistance);
                var avgDistance = Math.Round(trips.Average(y => y.DrivenDistance), 2);

                TotalDistanceActualMonth = totalThisMonth.ToString();
                TotalDistanceLastMonth = totalLastMonth.ToString();
                AvgDistance = avgDistance.ToString();

                Percentage = (double)Math.Round(totalThisMonth / MonthlyKilometers, 2);

                PropChanged(nameof(Percentage));
                PropChanged(nameof(TotalDistanceActualMonth));
                PropChanged(nameof(TotalDistanceLastMonth));
                PropChanged(nameof(AvgDistance));
            });
        }

        private void InitViewModel()
        {
            Task.Run(() =>
            {
                while (GlobalDataStore.DataItemManager == null || !GlobalDataStore.DataItemManager.IsLoaded)
                    Thread.Sleep(100);

                var trips = GlobalDataStore.DataItemManager.TripItems
                    .Select(x => x)
                    .Reverse();
                
                int index = 0;
                foreach(var tripItem in trips)
                    AddTripItemToObservableCollection(ref index, tripItem);

                PropChanged(nameof(TripItems));
                PropChanged(nameof(PageinatedTripItems));
            });
        }

        private void AddTripItemToObservableCollection(ref int index, TripItem tripItem)
        {
            if (_lastMonth == 0)
                _lastMonth = tripItem.Timestamp.Month;

            if (_lastMonth != tripItem.Timestamp.Month)
            {
                _currentPageinationSetIndex++;
                _lastMonth = tripItem.Timestamp.Month;
            }

            var topic = new TopicModelItem
            {
                Index = index++,
                ImageSource = "ElectricCarDriving.png",
                ItemInstance = tripItem,
                MonthCaption = tripItem.Timestamp.ToString("dd.MM.yyyy"),

                PageinationSetIndex = _currentPageinationSetIndex
            };
            TripItems.Add(topic);
        }
    }
}