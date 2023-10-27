using ErXZEService.Models;
using ErXZEService.Services.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ErXZEService.Services.CarDataPersistence.ElectricCarDataItemLoader
{
    /// <summary>
    /// Loads data from database path and from additional files
    /// </summary>
    public class ElectricCarDataItemLoaderHybrid : IElectricCarDataItemLoader
    {
        public Action<string> OnLoaderProgressChange { get; set; }

        private IElectricCarDataItemLoader DatabaseLoader { get; set; }
        private string FilesBasePath { get; set; }
        
        public ElectricCarDataItemLoaderHybrid(string databasePath, string filesBasePath, ILogger logger)
        {
            SetLoader(new ElectricCarDataItemLoaderFromDatabase(databasePath, logger), filesBasePath);
        }

        public ElectricCarDataItemLoaderHybrid(ElectricCarDataItemLoaderFromDatabase databaseLoader, string filesBasePath)
        {
            SetLoader(databaseLoader, filesBasePath);
        }

        public List<ElectricCarDataItem> Load()
        {
            DatabaseLoader.OnLoaderProgressChange = OnLoaderProgressChange;
            var result = DatabaseLoader.Load();

            var fileLoader = new ElectricCarDataItemLoaderFromFiles(new List<string>() { ElectricCarDataItemLoaderFromFiles.GetFilePaths(FilesBasePath).Last() })
            {
                OnLoaderProgressChange = OnLoaderProgressChange
            };
            //result.AddRange(fileLoader.Load());

            return result;
        }

        private void SetLoader(ElectricCarDataItemLoaderFromDatabase databaseLoader, string filesBasePath)
        {
            DatabaseLoader = databaseLoader;
            FilesBasePath = filesBasePath;
        }
    }
}
