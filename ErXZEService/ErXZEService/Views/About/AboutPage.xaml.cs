using ErXZEService.ViewModels.Settings;
using ErXZEService.Views.Settings;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ErXZEService.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void Settings_Clicked(object sender, EventArgs e)
        {
            Page myPage = new SettingsView();
            myPage.Title = "Settings";

            Navigation.PushAsync(myPage);
        }
    }
}