using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
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

        private DelegateCommand _connectCommand;
        private DelegateCommand _settingsCommand;
        private DelegateCommand _homePageCommand;

        private string _pageTitle;
        private IEnumerable<WidgetViewModelBase> _widgets;
        private CancellationTokenSource _loadingCancellationTokenSource;
        private HubPageParameters _parameters;
        private bool _isLoading;
        private string _lastUpdateTime;

        public HubPageViewModel(
            ISettingsManager settingsManager,
            INavigationService navigationService,
            IWidgetViewModelFactory widgetViewModelFactory)
        {
            _settingsManager = settingsManager;
            _navigationService = navigationService;
            _widgetViewModelFactory = widgetViewModelFactory;
        }

        public ICommand ConnectCommand
        {
            get { return (_connectCommand) ?? (_connectCommand = new DelegateCommand(LoadPage)); }
        }

        public ICommand SettingsCommand
        {
            get { return (_settingsCommand) ?? (_settingsCommand = new DelegateCommand(OpenSettingsPage)); }
        }
        public object HomePageCommand
        {
            get { return (_homePageCommand) ?? (_homePageCommand = new DelegateCommand(OpenHomePage)); }
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

        public IEnumerable<WidgetViewModelBase> Widgets
        {
            get { return _widgets; }
            private set { SetProperty(ref _widgets, value); }
        }

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

            _parameters = navigationParameter as HubPageParameters;

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

        }

        private void LoadPage()
        {
            var settings = _settingsManager.LoadSettings();
            if (settings == null)
            {
                //TODO: display error or navigate to config
                return;
            }

            var baseUri = settings.ResolveBaseUri();

            var client = new OpenHabRestClient(baseUri);

            _loadingCancellationTokenSource = new System.Threading.CancellationTokenSource();

            IsLoading = true;
            Task.Run(async () =>
            {
                var allSitempas = (await client.GetSitemaps(_loadingCancellationTokenSource.Token)).ToArray();
                var sitemap = allSitempas.FirstOrDefault(s => s.Name == settings.Sitemap) ?? 
                    allSitempas.FirstOrDefault();

                Page page = null;
                if (_parameters == null || _parameters.IsHomepage)
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

            var widgets = new List<WidgetViewModelBase>();
            foreach (var widget in page.Widgets)
            {
                var widgetViewModel = _widgetViewModelFactory.Create(widget.Type);
                widgetViewModel.Set(widget);
                widgets.Add(widgetViewModel);
            }

            Widgets = widgets;
        }

        private void OpenSettingsPage()
        {
            _navigationService.Navigate("Settings", null);
        }

        private void OpenHomePage()
        {

        }
    }
}
