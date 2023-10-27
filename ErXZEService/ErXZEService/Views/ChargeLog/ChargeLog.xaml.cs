using ErXZEService.DependencyServices;
using ErXZEService.Models;
using ErXZEService.Services;
using ErXZEService.Services.Log;
using ErXZEService.Services.SQL;
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
	public partial class ChargeLog : ContentPage
    {
        private ChargeItem _selectedChargeItem = null;
        private TopicModelItem _topic = null;

        public ChargeLog ()
		{
			InitializeComponent ();
		}

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            _topic = e.Item as TopicModelItem;

            if (_topic != null && _topic.ItemInstance is ChargeItem)
            {
                _selectedChargeItem = (ChargeItem)_topic.ItemInstance;

                Page myPage = new ChargeItemDetail();
                var vm = new ChargeModelItem(_selectedChargeItem);
                myPage.BindingContext = new ChargeItemViewModel(vm);
                myPage.Title = _selectedChargeItem.ChargeCaption;

                Navigation.PushAsync(myPage);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var logger = IoC.Resolve<ILogger>();
			var toaster = DependencyService.Get<IToaster>();

            try
            {
                if (_selectedChargeItem != null && _selectedChargeItem.Changed)
                {
                    if (BindingContext is ChargeLogViewModel vm)
                    {
                        vm.PropChanged(nameof(ChargeLogViewModel.ChargeItems));
                        vm.PropChanged(nameof(ChargeLogViewModel.PageinatedChargeItems));
                    }

                    using (var session = new SQLiteSession(logger))
                    {
                        _selectedChargeItem.Save(session);
                    }

					toaster?.Short("Changes saved successfully!");
                }
            }
            catch (Exception e)
            {
                logger.LogError("Failed to save Entity to Database on Change", e);

				toaster?.Long("Error while saving changes! " + e.ToString());
            }
            finally
            {
                _selectedChargeItem = null;
                _topic = null;
            }
        }
    }
}