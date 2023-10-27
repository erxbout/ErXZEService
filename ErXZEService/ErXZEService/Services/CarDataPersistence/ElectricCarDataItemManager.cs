using ErXZEService.Models;
using ErXZEService.Services.CarDataPersistence.ElectricCarDataItemLoader;
using ErXZEService.Services.CarDataPersistence.ElectricCarDataItemProcessing;
using ErXZEService.Services.Log;
using ErXZEService.Services.Paths;
using ErXZEService.Services.SQL;
using ErXZEService.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ErXZEService.Services.CarDataPersistence
{
    public class ElectricCarDataItemManager
    {
        private readonly ILogger _logger;

        public ChargeItem CurrentCharge => Appender.CurrentCharge;
        public TripItem CurrentTrip => Appender.CurrentTrip;

        public ChargeItem LastFinishedCharge => Appender.LastFinishedCharge;
        public TripItem LastFinishedTrip => Appender.LastFinishedTrip;

        public List<ChargeItem> ChargeItems => Appender.ChargeItems;
        public List<TripItem> TripItems => Appender.TripItems;

        public ElectricCarDataItem CurrentDataItem => Appender.CurrentDataItem;
        public DateTime CurrentDate => Appender.CurrentDate;

        public Action<string> OnPropertyChange { get; set; }

        public Action<ElectricCarState> OnCarStateChanged { get; set; }

        public Action<string> OnImportProgressChange { get; set; }

        public bool ReverseEngineer { get; set; }
        public bool IsLoaded { get; set; }
        public bool AppendingEnabled { get; set; }

        public ElectricCarDataItemAppender Appender { get; set; } = new ElectricCarDataItemAppender();

        private bool IsImporting { get; set; }

        public ElectricCarDataItemManager(ILogger logger)
        {
            AppendingEnabled = true;
            Appender.OnSave = x =>
            {
                logger.LogInformation($"Appender callback OnSave entity:{x}");

                using (var appenderSession = new SQLiteSession(logger))
                    appenderSession.SaveRecursive(x);
            };

            Appender.OnCarStateChanged = x => OnCarStateChanged?.Invoke(x);
            _logger = logger;
        }

        public void Append(string dataItemString)
        {
            if (AppendingEnabled)
                Append((ElectricCarDataItem)dataItemString);
        }

        public void Append(ElectricCarDataItem dataItem)
        {
            if (AppendingEnabled)
                Appender.Append(dataItem, IsImporting);
        }

        public void LoadOverviewData(SQLiteSession session)
        {
            ElectricCarDataItem dataItem;

            try
            {
                dataItem = session.SelectMany<ElectricCarDataItem>().Last();
            }
            catch (Exception e)
            {
                dataItem = ElectricCarDataItem.GetEmptyElectricCarDataItem();
                _logger.LogInformation("Cannot select dataItem, fallback to empty dataitem", e);
            }

            Appender.SetCurrentDataItem(dataItem);
            Appender.RestoreCurrentTripAndCharge();
            InvokePropertyChanged(nameof(CurrentDataItem));

            var lastChargeItem = session.SelectLastOrDefault<ChargeItem>();
            var lastTripItem = session.SelectLastOrDefault<TripItem>();

            session.SelectRecursive(lastChargeItem);
            session.SelectRecursive(lastTripItem);

            Appender.ChargeItems = new List<ChargeItem>() { lastChargeItem };
            Appender.TripItems = new List<TripItem>() { lastTripItem };

            InvokePropertyChanged(nameof(ChargeItems));
            InvokePropertyChanged(nameof(TripItems));
        }

        public void Load(SQLiteSession session)
        {
            LoadFromDatabase(session);
            IsLoaded = true;
        }

        public void Load(string path)
        {
            ImportFromFilePath(path);
            IsLoaded = true;
        }

        public void ImportFromFilePath(string path, IElectricCarDataItemLoader loader = null)
        {
            IsImporting = true;

            //Step 1 load data from files
            if (loader == null)
            {
                if (File.Exists(StoragePathProvider.DataItemDatabasePath))
                    loader = new ElectricCarDataItemLoaderHybrid(StoragePathProvider.DataItemDatabasePath, path, _logger);
                else
                    loader = new ElectricCarDataItemLoaderFromFiles(path, _logger, StoragePathProvider.NewSendPath);
            }

            loader.OnLoaderProgressChange = OnImportProgressChange;
            var dataItems = loader.Load();

            //Step 2 evenutally reverse engineer (save to file for later)
            if (ReverseEngineer)
            {
                OnImportProgressChange?.Invoke("Step 2: Reverse Engineer");
                new ElectricCarDataItemReverseEngineer()
                    .SaveDataItemsToFile("beforeReverseEngineered.txt", dataItems)
                    .ReverseEngineerTimestamps(dataItems)
                    .SaveToFile("reverseEngineered.txt");
            }

            OnImportProgressChange?.Invoke("Step 3: Save cache for later hybrid loading");
            //Step 3 save imported for later hybrid loading
            if (File.Exists(StoragePathProvider.DataItemDatabasePath))
            {
                File.Delete(StoragePathProvider.DataItemDatabasePath);

                //to ensure items get saved in new database because the entries do not exist to update when deleting and recreating
                dataItems.ForEach(x => x.Id = 0);
            }

            var lastDataSource = ElectricCarDataItemLoaderFromFiles.GetFileNames(path).Last();

            using (var session = new SQLiteSession(StoragePathProvider.DataItemDatabasePath, _logger))
                session.Save(dataItems.Where(x => x.DataSource != lastDataSource));

            OnImportProgressChange?.Invoke("Step 4: Processing Data-Items");
            //Step 4 process dataitems
            var percentageStep = dataItems.Count / 100;
            for (int i = 0; i < dataItems.Count; i++)
            {
                Profiler.MeasureCall(() => Append(dataItems[i]), nameof(Append));

                if (OnImportProgressChange != null && i % percentageStep == 0)
                {
                    var processingProgressInPercent = Math.Round(i / (decimal)dataItems.Count * 100, 0);
                    OnImportProgressChange.Invoke($"Step 4: Processing Data-Items ({processingProgressInPercent}%)");
                }
            }

            //reappend last dataitem to save it in db
            Appender.CurrentDataItem.Id = 0;
            Appender.CurrentDataItem.Changed = true;
            Appender.SaveCurrentDataItem();

            //Step 5 match item captions from last database
            if (File.Exists(StoragePathProvider.TodaysDatabasePath))
            {
                OnImportProgressChange?.Invoke("Step 5: Matching with last db");
                using (var originSession = new SQLiteSession(StoragePathProvider.TodaysDatabasePath, _logger))
                {
                    originSession.SelectMany<TripBReset>().ForEach(x => GlobalDataStore.OdoCalculator.AddTripBReset(x));
                    Appender.MatchItemCaptions(originSession.SelectMany<ChargeItem>());
                    Appender.MatchItemCaptions(originSession.SelectMany<TripItem>());
                }
            }

            OnImportProgressChange?.Invoke("Finish up by saving database");

            //Step 6 (final) save everything to database
            using (var session = new SQLiteSession(_logger))
            {
                session.SaveRecursive(TripItems);

                Profiler.MeasureCall(() => session.SaveRecursive(ChargeItems), "SaveChargeItems");
                GlobalDataStore.OdoCalculator.Save(session);
            }

            Profiler.PrintCallAverage(nameof(Append));
            Profiler.PrintCallAverage(nameof(CurrentDataItem.Merge));
            Profiler.PrintCallAverage("SaveChargeItems");

            IsImporting = false;
        }

        private void LoadFromDatabase(SQLiteSession session)
        {
            LoadOverviewData(session);

            var selectAndApplyTripBReset = Task.Run(() =>
            {
                try
                {
                    var resets = session.SelectMany<TripBReset>();
                    GlobalDataStore.OdoCalculator.SetTripBResets(resets);
                }
                catch (Exception e)
                {
                    _logger.LogInformation("Cannot select TripBReset", e);
                }
            });

            var chargeItems = new List<ChargeItem>();
            var tripItems = new List<TripItem>();

            var chargePoints = new List<ChargePoint>();
            var tripPoints = new List<TripPoint>();

            var selectChargeAndTripItems = Task.Run(() =>
            {
                chargeItems = session.SelectMany<ChargeItem>(limit:30);
                tripItems = session.SelectMany<TripItem>(limit: 100);
            });

            var selectChargeAndTripPoints = Task.Run(() =>
            {
                chargePoints = session.SelectMany<ChargePoint>();
                tripPoints = session.SelectMany<TripPoint>();
            });

            Task.WaitAll(selectAndApplyTripBReset, selectChargeAndTripItems, selectChargeAndTripPoints);

            var matchChargeItemsAndChargePoints = Task.Run(() =>
            {
                foreach (var item in chargeItems)
                {
                    item.ChargePoints = chargePoints.Where(x => x.ChargeItemId == item.Id).Take(10).ToList();
                    item.Trips = tripItems.Where(x => x.ChargeItemId == item.Id).ToList();
                }
            });

            var matchTripItemPoints = Task.Run(() =>
            {
                foreach (var item in tripItems)
                {
                    item.TripPoints = tripPoints.Where(x => x.TripItemId == item.Id).Take(10).ToList();
                }
            });

            Appender.CurrentTripItemCount = session.Count<TripItem>(x => true);

            Task.WaitAll(matchChargeItemsAndChargePoints, matchTripItemPoints);

            Appender.ChargeItems = chargeItems;
            Appender.TripItems = tripItems;

            InvokePropertyChanged(nameof(ChargeItems));
            InvokePropertyChanged(nameof(TripItems));
        }

        private void InvokePropertyChanged(string propertyName)
        {
            OnPropertyChange?.Invoke(propertyName);
        }
    }
}
