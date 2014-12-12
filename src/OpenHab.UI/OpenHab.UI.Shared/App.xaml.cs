using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Microsoft.Practices.Unity;
using OpenHab.UI.ViewModels;

namespace OpenHab.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : MvvmAppBase
    {
        private readonly IUnityContainer _container = new UnityContainer();
        private readonly IEventAggregator _eventAggregator = new EventAggregator();

        public IEventAggregator EventAggregator { get { return _eventAggregator; } }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            if (args != null && !string.IsNullOrEmpty(args.Arguments))
            {
                // The app was launched from a Secondary Tile 
                // Navigate to the item's page 
                NavigationService.Navigate("ItemDetail", args.Arguments);
            }
            else 
            { 
                // Navigate to the initial page 
                NavigationService.Navigate("Hub", null); 
            } 
            Window.Current.Activate(); 
            return Task.FromResult<object>(null);
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            _container.RegisterInstance<INavigationService>(NavigationService); 
            _container.RegisterInstance<ISessionStateService>(SessionStateService); 
            _container.RegisterInstance<IEventAggregator>(EventAggregator); 
         //   _container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));

            _container.RegisterType<IWidgetViewModelFactory, WidgetViewModelFactory>(new ContainerControlledLifetimeManager());

            ViewModelLocationProvider.SetDefaultViewModelFactory(Resolve);

            return base.OnInitializeAsync(args);
        }

        protected override object Resolve(Type type)
        {
            return _container.Resolve(type);
        }
    }
}