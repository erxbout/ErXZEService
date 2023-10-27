using DryIoc;
using ErXZEService.Models;
using ErXZEService.Services.Abrp;
using ErXZEService.Services.CarDataPersistence;
using ErXZEService.Services.ChargepointPolling;
using ErXZEService.Services.ChargepointPolling.Interfaces;
using ErXZEService.Services.Configuration;
using ErXZEService.Services.Events;
using ErXZEService.Services.Log;

namespace ErXZEService.Services
{
    public class GlobalDataStore
    {
        public delegate void TopicModelAddedEventHandler(ITopicModelItem item);
        public static event TopicModelAddedEventHandler TopicModelItemAdded;

        public static void AddedTopicModelItem(ITopicModelItem item)
        {
            TopicModelItemAdded?.Invoke(item);
        }

        public static OdometerCalculator OdoCalculator = new OdometerCalculator(IoC.Resolve<ILogger>());

        public static ElectricCarDataItemManager DataItemManager;
    }

    public class IoC
    {
        private static Container _iocContainer;

        public static Container IoContainer
        {
            get
            {
                return _iocContainer;
            }
            set 
            {
                if (_iocContainer == null)
                    _iocContainer = value;
            }
        }

        /// <summary>
        /// Initializes the IoC Container (IoContainer)
        /// </summary>
        public static void InitIoContainer()
        {
            IoContainer = new Container();
            IoContainer.Register<ILogger, Logger>(new SingletonReuse());
            IoContainer.Register<IConfiguration, Configuration.Configuration>(new SingletonReuse());
            IoContainer.Register<IReadonlyConfiguration, Configuration.Configuration>(new SingletonReuse());

            IoContainer.Register<IEventService, EventService>(new SingletonReuse());

            IoContainer.Register<IAbrpTelemetryService, AbrpTelemetryService>(new SingletonReuse());
            IoContainer.Register<IEnergieSteiermarkChargepointPoller, EnergieSteiermarkChargepointPoller>(new SingletonReuse());
        }

        public static T Resolve<T>()
        {
            if (IoContainer == null)
                InitIoContainer();

            return IoContainer.Resolve<T>(IfUnresolved.ReturnDefault);
        }
    }
}
