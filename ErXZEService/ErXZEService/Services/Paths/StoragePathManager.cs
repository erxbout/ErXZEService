using ErXZEService.DependencyServices;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace ErXZEService.Services.Paths
{
    public enum StoragePath
    {
        Base,
        DatabasePath,
        DataItemDatabasePath,
        TodayDatabasePath,
        NewSendPath,
        LogSendPath,
        ImportPath
    }

    public class StoragePathManager
    {
        private static bool _storagePathServiceInitialized;
        private static IStoragePathService _storagePathService;

        private static Dictionary<StoragePath, string> RelativePaths = new Dictionary<StoragePath, string>()
        {
            { StoragePath.Base, "" },
            { StoragePath.DatabasePath, "zSql.db" },
            { StoragePath.DataItemDatabasePath, "zImport.db" },
            { StoragePath.TodayDatabasePath, "zSql{0}.db" },
            { StoragePath.NewSendPath, "SND_New.TXT" },
            { StoragePath.LogSendPath, "SND_Log.TXT" },
            { StoragePath.ImportPath, "SND_IMP{0}.TXT" },
        };

        private static void InitStoragePathService()
        {
            if (_storagePathServiceInitialized)
                return;

            try
            {
                _storagePathService = DependencyService.Get<IStoragePathService>();
                _storagePathServiceInitialized = true;
            }
            catch
            {
                _storagePathServiceInitialized = true;
            }
        }

        public static string PathPrefix
        {
            get
            {
                InitStoragePathService();
                if (_storagePathService != null)
                    return Path.Combine(_storagePathService.ExternalStorage, RelativePaths[StoragePath.Base]);

                return "";
            }
        }

        public static string GetPath(StoragePath path)
        {
            return Path.Combine(PathPrefix, RelativePaths[path]);
        }

        public static string GetPathFormatted(StoragePath path, params string[] formatArgs)
        {
            return Path.Combine(PathPrefix, string.Format(RelativePaths[path], formatArgs));
        }
    }
}
