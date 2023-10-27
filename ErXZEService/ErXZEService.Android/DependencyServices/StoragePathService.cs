using ErXZEService.DependencyServices;
using ErXZEService.Droid.DependencyServices;
using System;

[assembly: Xamarin.Forms.Dependency(typeof(StoragePathService))]

namespace ErXZEService.Droid.DependencyServices
{
    public class StoragePathService : IStoragePathService
    {
        public string ExternalStorage => Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

        public string AppStorage => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
}