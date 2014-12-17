using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using OpenHab.Client;
using OpenHab.UI.Services;

namespace OpenHab.UI.ViewModels
{
    public class HubPageParameters
    {
        public bool IsHomepage { get; set; }
    }

    public class HubPageViewModel : ViewModel
    {
        private readonly ISettingsManager _settingsManager;
        private readonly INavigationService _navigationService;
        private readonly IWidgetViewModelFactory _widgetViewModelFactory;
        private readonly IEventAggregator _eventAggregator;

        private DelegateCommand _connectCommand;
        private DelegateCommand _settingsCommand;
        private DelegateCommand _homepageCommand;

        private string _pageTitle;
        private ObservableCollection<WidgetViewModelBase> _widgets;
        private CancellationTokenSource _loadingCancellationTokenSource;
        private HubPageParameters _parameters;
        private bool _isLoading;
        private string _lastUpdateTime;
        private bool _isHomepage;

        public HubPageViewModel(
            ISettingsManager settingsManager,
            INavigationService navigationService,
            IWidgetViewModelFactory widgetViewModelFactory,
            IEventAggregator eventAggregator)
        {
            _settingsManager = settingsManager;
            _navigationService = navigationService;
            _widgetViewModelFactory = widgetViewModelFactory;
            _eventAggregator = eventAggregator;
        }

        public ICommand ConnectCommand
        {
            get { return (_connectCommand) ?? (_connectCommand = new DelegateCommand(LoadPage)); }
        }

        public ICommand SettingsCommand
        {
            get { return (_settingsCommand) ?? (_settingsCommand = new DelegateCommand(OpenSettingsPage)); }
        }
        public object HomepageCommand
        {
            get { return (_homepageCommand) ?? (_homepageCommand = new DelegateCommand(OpenHomePage)); }
        }

        public bool IsHomepage
        {
            get { return _isHomepage; }
            protected set { SetProperty(ref _isHomepage, value); }
        }

        public string PageTitle
        {
            get { return _pageTitle; }
            protected set { SetProperty(ref _pageTitle, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            protected set { SetProperty(ref _isLoading, value); }
        }

        public string LastUpdateTime
        {
            get { return _lastUpdateTime; }
            protected set { SetProperty(ref _lastUpdateTime, value); }
        }

        public ObservableCollection<WidgetViewModelBase> Widgets
        {
            get { return _widgets; }
            private set { SetProperty(ref _widgets, value); }
        }

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

            _parameters = navigationParameter as HubPageParameters;
            IsHomepage = (_parameters == null || _parameters.IsHomepage);

            _eventAggregator.GetEvent<SettingsChangedEvent>()
                .Subscribe(OnSettingsChanged, ThreadOption.UIThread, false);

            LoadPage();
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);

            if (_loadingCancellationTokenSource != null)
            {
                _loadingCancellationTokenSource.Cancel();
                _loadingCancellationTokenSource = null;
            }

            _eventAggregator.GetEvent<SettingsChangedEvent>()
                .Unsubscribe(OnSettingsChanged);
        }

        private void OnSettingsChanged(Settings settings)
        {

        }


        private void LoadPage()
        {
            var settings = _settingsManager.CurrentSettings;
            if (settings == null)
            {
                //TODO: display error or navigate to config
                return;
            }

            var baseUri = settings.ResolveBaseUri();
            var client = new OpenHabRestClient(baseUri);

            _loadingCancellationTokenSource = new CancellationTokenSource();

            IsLoading = true;
            Task.Run(async () =>
            {
                var allSitempas = (await client.GetSitemaps(_loadingCancellationTokenSource.Token)).ToArray();
                var sitemap = allSitempas.FirstOrDefault(s => s.Name == settings.Sitemap) ?? 
                    allSitempas.FirstOrDefault();

                Page page = null;
                if (IsHomepage)
                    page = (await client.GetPage(sitemap.Homepage, _loadingCancellationTokenSource.Token));
                
                //TODO: load other page
                
                return page;

            }, _loadingCancellationTokenSource.Token)
            .ContinueWith(t =>
            {
                IsLoading = false;
                LastUpdateTime = DateTime.Now.ToString("HH:mm:ss.fff");

                if (t.IsCanceled)
                    return;
                if (t.IsFaulted)
                    return;
                
                var page = t.Result;
                ProcessPage(page);

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ProcessPage(Page page)
        {
            PageTitle = page.Title;

            // check if collection has changed
            bool hasChanged = false;
            if (_widgets == null || _widgets.Count != page.Widgets.Count)
            {
                hasChanged = true;
            }
            else
            {
                for (int index = 0; index < page.Widgets.Count; index++)
                {
                    if (_widgets[index].WidgetId != page.Widgets[index].WidgetId)
                    {
                        hasChanged = true;
                        break;
                    }
                }
            }

            // use existing, or rebuild if has changed
            var widgets = (_widgets != null && !hasChanged) ? _widgets : new ObservableCollection<WidgetViewModelBase>();

            for (int index = 0; index < page.Widgets.Count; index++)
            {
                var widget = page.Widgets[index];

                WidgetViewModelBase vm;
                if (index < widgets.Count)
                {
                    // update existing
                    vm = widgets[index];
                }
                else
                {
                    // create new 
                    vm = _widgetViewModelFactory.Create(widget.Type);
                    widgets.Insert(index, vm);
                }

                vm.Update(widget);
            }

            Widgets = widgets;
        }

        private void OpenSettingsPage()
        {
            _navigationService.Navigate(PageToken.Settings, null);
        }

        private void OpenHomePage()
        {
            _navigationService.Navigate(PageToken.Hub, new HubPageParameters{IsHomepage = true});
        }
    }
}
