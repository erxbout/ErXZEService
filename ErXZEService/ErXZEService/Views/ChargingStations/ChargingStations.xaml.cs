using ErXZEService.Models;
using ErXZEService.ViewModelItems;
using ErXZEService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ErXZEService.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ChargeStations : ContentPage
	{
		public ChargeStations()
		{
			InitializeComponent ();
		}

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            
        }
    }
}