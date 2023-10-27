using System;

namespace ErXZEService.Services.Paths
{
    public class StoragePathProvider
    {
        public static string BaseStoragePath { get => StoragePathManager.PathPrefix; }

        public static string DatabasePath { get => StoragePathManager.GetPath(StoragePath.DatabasePath); }
        public static string DataItemDatabasePath { get => StoragePathManager.GetPath(StoragePath.DataItemDatabasePath); }
        public static string TodaysDatabasePath { get => StoragePathManager.GetPathFormatted(StoragePath.TodayDatabasePath, DateTime.Now.ToString("yyyyMMdd")); }

        public static string NewSendPath { get => StoragePathManager.GetPath(StoragePath.NewSendPath); }
        public static string LogSendPath { get => StoragePathManager.GetPath(StoragePath.LogSendPath); }
        public static string ImportFilePath { get => StoragePathManager.GetPathFormatted(StoragePath.ImportPath, DateTime.Now.ToString("yyyyMMdd")); }
    }
}
