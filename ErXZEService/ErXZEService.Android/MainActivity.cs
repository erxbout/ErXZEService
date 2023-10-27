using System;
using System.IO;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.OS;

namespace ErXZEService.Droid
{
    [Activity(Label = "ErXZEService", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
            LoadApplication(new App());
        }

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            string filePath = "/storage/emulated/0/APPCRASH.txt";
            System.Collections.Generic.List<string> toAppend = new System.Collections.Generic.List<string>()
            {
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
                e.Exception.Message,
                e.Exception.StackTrace
            };

            try { File.AppendAllLines(filePath, toAppend); } 
            catch { }
        }
    }
}