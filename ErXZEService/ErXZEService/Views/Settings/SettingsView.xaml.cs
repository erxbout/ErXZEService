using ErXZEService.ViewModels.Settings;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ErXZEService.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsView : ContentPage
	{
		public SettingsView ()
		{
			InitializeComponent ();
            
            BindingContext = new SettingsViewModel(this);
        }
	}
}