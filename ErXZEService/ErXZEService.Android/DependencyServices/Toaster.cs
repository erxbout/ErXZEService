using ErXZEService.DependencyServices;
using ErXZEService.Droid.DependencyServices;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(Toaster))]

namespace ErXZEService.Droid.DependencyServices
{
    public class Toaster : IToaster
    {
        public void Long(string message)
        {
            MakeText(message, ToastLength.Long);
        }

        public void Short(string message)
        {
            MakeText(message, ToastLength.Short);
        }

        private static void MakeText(string message, ToastLength toastLength)
        {
            Toast.MakeText(Android.App.Application.Context, message, toastLength).Show();
        }
    }
}