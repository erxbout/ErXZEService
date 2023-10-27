using ErXZEService.Models;
using ErXZEService.Services.Paths;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ErXZEService.Services.CarDataPersistence.ElectricCarDataItemProcessing
{
    public class ElectricCarDataItemReverseEngineer
    {
        private List<ElectricCarDataItem> ProcessedDataItems { get; set; }

        public ElectricCarDataItemReverseEngineer ReverseEngineerTimestamps(List<ElectricCarDataItem> items)
        {
            ElectricCarDataItem lastItemWithSpecifiedTimestamp = items.LastOrDefault(x => x.HasSpecifiedTimestamp);
            var indexOfItemForMissingLdk = 0;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];

                if (!item.IsFullySpecified)
                    continue;

                if (item.HasSpecifiedTimestamp)
                    lastItemWithSpecifiedTimestamp = item;
                else
                {
                    if (lastItemWithSpecifiedTimestamp == null)
                        continue;

                    if (item.ArduinoTimeSinceIgnition <= lastItemWithSpecifiedTimestamp.ArduinoTimeSinceIgnition)
                    {
                        var dif = item.ArduinoTimeSinceIgnition - lastItemWithSpecifiedTimestamp.ArduinoTimeSinceIgnition;
                        item.Timestamp = lastItemWithSpecifiedTimestamp.Timestamp.Value.AddMinutes(dif);
                        item.HasSpecifiedTimestamp = true;
                        lastItemWithSpecifiedTimestamp = item;
                    }
                    else
                    {
                        lastItemWithSpecifiedTimestamp = null;
                    }
                }

                if (i < items.Count - 1)
                {
                    if (item.SoC == -1)
                    {
                        item.SoC = items[i + 1].SoC;
                    }

                    if (item.AvaliableEnergy == -0.1m)
                    {
                        item.AvaliableEnergy = items[i + 1].AvaliableEnergy;
                    }
                }

                if (indexOfItemForMissingLdk > 0 && item.StateNumber.HasValue && item.State == ElectricCarState.Parked)
                {
                    var chargedKwh = items[indexOfItemForMissingLdk].AvaliableEnergy - lastItemWithSpecifiedTimestamp.AvaliableEnergy;

                    if (chargedKwh > 0)
                    {
                        items[indexOfItemForMissingLdk].ChargedKWH = chargedKwh;
                        items[indexOfItemForMissingLdk].ChargedRange = (short)(items[indexOfItemForMissingLdk].EstimatedRange - lastItemWithSpecifiedTimestamp.EstimatedRange);
                        items[indexOfItemForMissingLdk].TimeSinceLastEvent = (short)(items[indexOfItemForMissingLdk].ArduinoTimeSinceIgnition - lastItemWithSpecifiedTimestamp.ArduinoTimeSinceIgnition);
                        items[indexOfItemForMissingLdk].PilotAmpere = 0;
                        items[indexOfItemForMissingLdk].StateNumber = 1;
                    }

                    indexOfItemForMissingLdk = 0;
                }

                if (item.SoC == 100 && item.ChargedKWH == null && i > 0 && (items[i - 1].PilotAmpere.GetValueOrDefault(0) > 0 || items[i - 1].PilotAmpere.GetValueOrDefault(0) > 0))
                {
                    indexOfItemForMissingLdk = i - 1;
                }
            }

            ProcessedDataItems = items;
            return this;
        }

        public void SaveToFile(string path)
        {
            SaveDataItemsToFile(path, ProcessedDataItems);
        }

        public ElectricCarDataItemReverseEngineer SaveDataItemsToFile(string path, List<ElectricCarDataItem> list)
        {
            path = Path.Combine(StoragePathProvider.BaseStoragePath, path);

            if (list != null)
                if (!File.Exists(path))
                    File.WriteAllLines(path, list.Select(x => x.ToString()));

            return this;
        }
    }
}
