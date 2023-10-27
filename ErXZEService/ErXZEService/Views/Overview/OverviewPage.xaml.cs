using ErXZEService.Models;
using ErXZEService.Services;
using ErXZEService.ViewModelItems;
using ErXZEService.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ErXZEService.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OverviewPage : ContentPage
    {
        public OverviewPage()
        {
            InitializeComponent();

            BindingContext = new OverviewViewModel(this);
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var topic = e.Item as TopicModelItem;

            if (topic != null && topic.ItemInstance is ChargeItem chargeItem)
            {
                Page myPage = new ChargeItemDetail();
                myPage.BindingContext = new ChargeItemViewModel(new ChargeModelItem(chargeItem));
                myPage.Title = chargeItem.ChargeCaption;

                Navigation.PushAsync(myPage);
            }

            if (topic != null && topic.ItemInstance is OverviewViewModel viewModel)
            {
                viewModel.PushNavigationToLiveView(viewModel.CarData.DataItem.State, this);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is OverviewViewModel)
            {
                GlobalDataStore.DataItemManager.Appender.StoreCurrentTripAndCharge();
            }
        }
    }
}