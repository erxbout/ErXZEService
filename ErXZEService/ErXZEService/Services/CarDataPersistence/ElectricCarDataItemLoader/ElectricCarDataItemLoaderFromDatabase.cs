using System;
using System.Collections.Generic;
using ErXZEService.Models;
using ErXZEService.Services.Log;
using ErXZEService.Services.SQL;

namespace ErXZEService.Services.CarDataPersistence.ElectricCarDataItemLoader
{
    public class ElectricCarDataItemLoaderFromDatabase : IElectricCarDataItemLoader
    {
        private readonly ILogger _logger;

        public Action<string> OnLoaderProgressChange { get; set; }

        private string DatabasePath { get; set; }

        public ElectricCarDataItemLoaderFromDatabase(string databasePath, ILogger logger)
        {
            DatabasePath = databasePath;
            _logger = logger;
        }

        public List<ElectricCarDataItem> Load()
        {
            var result = new List<ElectricCarDataItem>();

            using (var session = new SQLiteSession(DatabasePath, _logger))
            {
                try
                {
                    result = session.SelectMany<ElectricCarDataItem>();
                }
                catch (Exception e)
                {
                    _logger.LogError("Cannot select ElectricCarDataItems", e);
                }
            }

            return result;
        }
    }
}
