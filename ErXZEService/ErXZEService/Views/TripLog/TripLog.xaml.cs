using ErXZEService.DependencyServices;
using ErXZEService.Models;
using ErXZEService.Services;
using ErXZEService.Services.Log;
using ErXZEService.Services.SQL;
using ErXZEService.ViewModelItems;
using ErXZEService.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ErXZEService.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TripLog : ContentPage
    {
        TripItem selectedTripItem = null;
        TopicModelItem topic = null;

        public TripLog()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            topic = e.Item as TopicModelItem;

            if (topic != null && topic.ItemInstance is TripItem)
            {
                selectedTripItem = (TripItem)topic.ItemInstance;

                Page myPage = new TripItemDetail();
                myPage.BindingContext = new TripItemViewModel(new TripModelItem(selectedTripItem));
                myPage.Title = selectedTripItem.Caption;

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
                if (selectedTripItem != null && selectedTripItem.Changed)
                {
                    if (BindingContext is TripLogViewModel vm)
                    {
                        vm.PropChanged(nameof(TripLogViewModel.TripItems));
                        vm.PropChanged(nameof(TripLogViewModel.PageinatedTripItems));
                    }

                    using (SQLiteSession session = new SQLiteSession(logger))
                    {
                        selectedTripItem.Save(session);
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
                selectedTripItem = null;
                topic = null;
            }
        }
    }
}