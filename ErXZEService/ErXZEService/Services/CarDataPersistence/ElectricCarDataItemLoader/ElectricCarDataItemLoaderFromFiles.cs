using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErXZEService.Models;
using ErXZEService.Services.Log;

namespace ErXZEService.Services.CarDataPersistence.ElectricCarDataItemLoader
{
    public class ElectricCarDataItemLoaderFromFiles : IElectricCarDataItemLoader
    {
        private readonly ILogger _logger;

        public Action<string> OnLoaderProgressChange { get; set; }

        private IEnumerable<string> Files { get; set; }
        private List<ElectricCarDataItem> CachedDataItems { get; set; }

        public ElectricCarDataItemLoaderFromFiles(string filePath, ILogger logger, string newSendPath = null)
        {
            SetFiles(GetFilePaths(filePath, newSendPath));
            _logger = logger;
        }

        public ElectricCarDataItemLoaderFromFiles(IEnumerable<string> files)
        {
            SetFiles(files);
        }

        public List<ElectricCarDataItem> Load()
        {
            if (CachedDataItems != null)
                return CachedDataItems;

            if (Files.Any())
                OnLoaderProgressChange?.Invoke($"found {Files.Count()} files..");

            var dataItems = new List<ElectricCarDataItem>();
            //prepare from files
            foreach (var file in Files)
            {
                try
                {
                    var fileContent = File.ReadAllLines(file);
                    var fileInfo = new FileInfo(file);

                    OnLoaderProgressChange?.Invoke($"Loading: {fileInfo.Name}");

                    dataItems.AddRange(
                        fileContent
                        .Where(x => x != string.Empty)
                        .Select(x => ElectricCarDataItem.TryParse(x, fileInfo.Name))
                        .Where(x => x != null)
                        .ToList());
                }
                catch (Exception e)
                {
                    _logger.LogError("Cannot Load data from: " + file, e);
                }
            }

            CachedDataItems = dataItems;
            return dataItems;
        }

        public static IEnumerable<string> GetFilePaths(in string path, string newSendPath = null)
        {
            if (newSendPath == null)
                newSendPath = string.Empty;

            var files = Directory
                .GetFiles(path)
                .Where(x => x.Contains("dSND_") || x == newSendPath)
                .OrderBy(y => y);

            return files;
        }
        
        public static IEnumerable<string> GetFileNames(in string path, string newSendPath = null)
        {
            var files = GetFilePaths(path, newSendPath);

            return files.Select(x => new FileInfo(x).Name);
        }

        private void SetFiles(IEnumerable<string> files)
        {
            Files = files;
        }
    }
}
