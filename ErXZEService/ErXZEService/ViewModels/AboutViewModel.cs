using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace ErXZEService.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";

            OpenWebCommand = new Command(() => Device.OpenUri(new Uri("https://erxbout.at")));
        }

        public ICommand OpenWebCommand { get; }
    }
}