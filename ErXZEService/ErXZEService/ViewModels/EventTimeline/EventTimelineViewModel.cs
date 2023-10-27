using ErXBoutCode.MVVM;
using ErXZEService.Models;
using ErXZEService.Services;
using ErXZEService.Services.Events;
using Microcharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ErXZEService.ViewModels
{
    public class EventTimelineViewModel : BaseViewModel, IDisposable
    {
        public ActionCommand NextPage { get; set; }
        public ActionCommand PreviousPage { get; set; }

        /// <summary>
        /// this is for storing the pageination set index for adding to the collection
        /// </summary>
        private int _currentPageinationSetIndex = 1;
        private readonly IEventService _eventService;

        public List<EventModelItem> Events { get; set; } = new List<EventModelItem>();

        public EventTimelineViewModel()
        {
            _eventService = IoC.Resolve<IEventService>();

            InitViewModel();
            Title = "Event Timeline";
        }

        public void Dispose()
        {
        }

        private void InitViewModel()
        {
            Task.Run(() =>
            {
                while (GlobalDataStore.DataItemManager == null || !GlobalDataStore.DataItemManager.IsLoaded)
                    Thread.Sleep(100);

                _eventService.LoadLatest();
                Events = _eventService.LatestEvents.OrderByDescending(x => x.Timestamp).Select(x => new EventModelItem(x)).ToList();
                

                PropChanged(nameof(Events));
            });
        }
    }
}