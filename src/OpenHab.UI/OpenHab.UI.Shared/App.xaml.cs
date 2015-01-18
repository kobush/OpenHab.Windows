using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Microsoft.Practices.Unity;

using OpenHab.UI.Services;
using OpenHab.UI.ViewModels;

namespace OpenHab.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : MvvmAppBase
    {
        private readonly IUnityContainer _container = new UnityContainer();
        private IEventAggregator _eventAggregator;

        private TransitionCollection _transitions;

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
                // check if app has settings
                var settingsMgr = _container.Resolve<ISettingsManager>();
                var settings = settingsMgr.LoadSettings();
                if (settings == null)
                {
                    // On first run navigate to the settings page
                    NavigationService.Navigate("Settings", true);
                }
                else
                {
                    // Navigate to the home page 
                    NavigationService.Navigate("Hub", new HubPageParameters { IsHomepage = true });
                }
            }

#if WINDOWS_PHONE_APP
            var rootFrame = (Frame)Window.Current.Content;
            // Removes the turnstile navigation for startup.
            if (rootFrame.ContentTransitions != null)
            {
                _transitions = new TransitionCollection();
                foreach (var c in rootFrame.ContentTransitions)
                {
                    _transitions.Add(c);
                }
            }

            rootFrame.ContentTransitions = null;
            rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

            Window.Current.Activate();
            return Task.FromResult<object>(null);
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = _transitions ?? new TransitionCollection { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // must be initialized on UI thread
            _eventAggregator = new EventAggregator();

            _container.RegisterInstance<INavigationService>(NavigationService);
            _container.RegisterInstance<ISessionStateService>(SessionStateService);
            _container.RegisterInstance<IEventAggregator>(EventAggregator);
            //   _container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));

            _container.RegisterType<ISettingsManager, SettingsManager>(new ContainerControlledLifetimeManager());
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