using ErXBoutCode.MVVM;
using ErXZEService.Models;
using ErXZEService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ErXZEService.ViewModels
{
    public class ChargeLogViewModel : BaseViewModel, IDisposable
    {
        public string TotalKWHActualMonth { get; set; }
        public string TotalKWHLastMonth { get; set; }
        public string AvgChargeKWH { get; set; }

        public ActionCommand NextPage { get; set; }
        public ActionCommand PreviousPage { get; set; }

        public string CurrentPageText => "Current Page: " + CurrentPage;

        private int CurrentPage { get; set; } = 1;
        private int _lastMonth;

        /// <summary>
        /// this is for storing the pageination set index for adding to the collection
        /// </summary>
        private int _currentPageinationSetIndex = 1;

        public List<TopicModelItem> ChargeItems { get; set; } = new List<TopicModelItem>();

        public List<TopicModelItem> PageinatedChargeItems
        {
            get
            {
                var result = ChargeItems
                    .SkipWhile(x => x.PageinationSetIndex < CurrentPage)
                    .TakeWhile(x => x.PageinationSetIndex == CurrentPage)
                    .ToList();

                return result;
            }
        }

        public ChargeLogViewModel()
        {
            NextPage = new ActionCommand(() =>
            {
                if (CurrentPage < ChargeItems.Max(x => x.PageinationSetIndex))
                    CurrentPage++;
                PropChanged(nameof(PageinatedChargeItems));
                PropChanged(nameof(CurrentPageText));
            });

            PreviousPage = new ActionCommand(() =>
            {
                if (CurrentPage > 1)
                    CurrentPage--;
                PropChanged(nameof(PageinatedChargeItems));
                PropChanged(nameof(CurrentPageText));
            });

            InitViewModel();
            CalculateKwh();
            Title = "Log";

            GlobalDataStore.TopicModelItemAdded += GlobalDataStore_TopicModelItemAdded;
        }

        public void Dispose()
        {
            GlobalDataStore.TopicModelItemAdded -= GlobalDataStore_TopicModelItemAdded;
        }

        private void GlobalDataStore_TopicModelItemAdded(ITopicModelItem item)
        {
            if (item is ChargeItem chargeItem)
            {
                var lastIndex = ChargeItems.Count == 0 ? 0 : ChargeItems.Max(x => x.Index);
                AddChargeItemToObservableCollection(ref lastIndex, chargeItem);

                ChargeItems.OrderByDescending(x => x.Index);

                PropChanged(nameof(ChargeItems));
                PropChanged(nameof(PageinatedChargeItems));
                CalculateKwh();
            }
        }

        private void CalculateKwh()
        {
            Task.Run(() =>
            {
                while (GlobalDataStore.DataItemManager == null)
                    Thread.Sleep(100);

                var chargeItems = GlobalDataStore.DataItemManager.ChargeItems;

                var totalKwhActualMonth = chargeItems.Where(x => x.Timestamp.Month == DateTime.Now.Month).Sum(y => y.ChargedKWH);
                var totalKwhLastMonth = chargeItems.Where(x => x.Timestamp.Month == DateTime.Now.AddMonths(-1).Month).Sum(y => y.ChargedKWH);
                var avgChargeKwh = Math.Round(chargeItems.Average(y => y.ChargedKWH), 2);

                TotalKWHActualMonth = totalKwhActualMonth.ToString();
                TotalKWHLastMonth = totalKwhLastMonth.ToString();
                AvgChargeKWH = avgChargeKwh.ToString();

                PropChanged(nameof(TotalKWHActualMonth));
                PropChanged(nameof(TotalKWHLastMonth));
                PropChanged(nameof(AvgChargeKWH));
            });
        }

        private void InitViewModel()
        {
            Task.Run(() =>
            {
                while (GlobalDataStore.DataItemManager == null || !GlobalDataStore.DataItemManager.IsLoaded)
                    Thread.Sleep(100);

                var chargeItems = GlobalDataStore.DataItemManager.ChargeItems
                    .Select(x => x)
                    .Reverse();

                var index = 0;
                foreach (var chargeItem in chargeItems)
                    AddChargeItemToObservableCollection(ref index, chargeItem);

                PropChanged(nameof(ChargeItems));
                PropChanged(nameof(PageinatedChargeItems));
            });
        }

        private void AddChargeItemToObservableCollection(ref int index, ChargeItem chargeItem)
        {
            if (_lastMonth == 0)
                _lastMonth = chargeItem.Timestamp.Month;

            if (_lastMonth != chargeItem.Timestamp.Month)
            {
                _currentPageinationSetIndex++;
                _lastMonth = chargeItem.Timestamp.Month;
            }

            var topic = new TopicModelItem
            {
                Index = index++,
                ImageSource = "BatteryChargingSymbol.png",
                ItemInstance = chargeItem,
                MonthCaption = chargeItem.Timestamp.ToString("dd.MM.yyyy"),

                PageinationSetIndex = _currentPageinationSetIndex
            };
            ChargeItems.Add(topic);
        }
    }
}