using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using MetroLog;
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
        public Page Page { get; set; }
    }

    public class HubPageViewModel : ViewModel
    {
        #region Fields

        private const string DefaultFrameId = "__default_frame__";
        private readonly ILogger Log;

        private readonly ISettingsManager _settingsManager;
        private readonly IConnectionTracker _connectionTracker;
        private readonly INavigationService _navigationService;
        private readonly IWidgetViewModelFactory _widgetViewModelFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly IPromptService _promptService;

        private DelegateCommand _connectCommand;
        private DelegateCommand _settingsCommand;
        private DelegateCommand _homepageCommand;
        private ICommand _openSitemapDialogCommand;
        private ICommand _sitemapAcceptCommand;
        private ICommand _sitemapCancelCommand;

        private string _pageTitle;
        private ObservableCollection<FrameWidgetViewModel> _frames;
        private CancellationTokenSource _loadingCancellationTokenSource;
        private HubPageParameters _parameters;
        private bool _isLoading;
        private string _lastUpdateTime;
        private bool _isHomepage;
        private bool _isConnecting;
        private Sitemap _selectedSitemap;
        private bool _showSitemapDialog;

        private Page _currentPage;

        #endregion

        public HubPageViewModel(
            ILogManager logManager,
            ISettingsManager settingsManager,
            IConnectionTracker connectionTracker,
            INavigationService navigationService,
            IWidgetViewModelFactory widgetViewModelFactory,
            IEventAggregator eventAggregator,
            IPromptService promptService)
        {
            Log = logManager.GetLogger<HubPageViewModel>();

            _settingsManager = settingsManager;
            _connectionTracker = connectionTracker;
            _navigationService = navigationService;
            _widgetViewModelFactory = widgetViewModelFactory;
            _eventAggregator = eventAggregator;
            _promptService = promptService;
        }

        #region Commands

        public ICommand ConnectCommand
        {
            get { return (_connectCommand) ?? (_connectCommand = new DelegateCommand(ReloadPage)); }
        }

        public ICommand OpenSettingsCommand
        {
            get { return (_settingsCommand) ?? (_settingsCommand = new DelegateCommand(OpenSettingsPage)); }
        }

        public ICommand OpenHomepageCommand
        {
            get { return (_homepageCommand) ?? (_homepageCommand = new DelegateCommand(OpenHomePage)); }
        }

        public ICommand OpenSitemapDialogCommand
        {
            get { return (_openSitemapDialogCommand) ?? (_openSitemapDialogCommand = new DelegateCommand(() => LoadSitemaps(false))); }
        }

        public ICommand SitemapAcceptCommand
        {
            get { return (_sitemapAcceptCommand) ?? (_sitemapAcceptCommand = new DelegateCommand(AcceptSitemapDialog)); }
        }
        public ICommand SitemapCancelCommand
        {
            get { return (_sitemapCancelCommand) ?? (_sitemapCancelCommand = new DelegateCommand(CloseSitemapDialog)); }
        }

        #endregion

        #region Properties

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

        public bool IsConnecting
        {
            get { return _isConnecting; }
            private set { SetProperty(ref _isConnecting, value); }
        }

        public string LastUpdateTime
        {
            get { return _lastUpdateTime; }
            protected set { SetProperty(ref _lastUpdateTime, value); }
        }

        public ObservableCollection<FrameWidgetViewModel> Frames
        {
            get { return _frames; }
            private set { SetProperty(ref _frames, value); }
        }

        public IEnumerable<Sitemap> Sitemaps { get; set; }

        public Sitemap SelectedSitemap
        {
            get { return _selectedSitemap; }
            set { SetProperty(ref _selectedSitemap, value); }
        }

        public bool ShowSitemapDialog
        {
            get { return _showSitemapDialog; }
            private set { SetProperty(ref _showSitemapDialog, value); }
        }

        #endregion

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

            _parameters = navigationParameter as HubPageParameters;
            IsHomepage = (_parameters == null || _parameters.IsHomepage);

            _eventAggregator.GetEvent<SettingsChangedEvent>().Subscribe(OnSettingsChanged, ThreadOption.UIThread, false);
            _eventAggregator.GetEvent<OpenHabConnected>().Subscribe(OnConnected, ThreadOption.UIThread, false);
            _eventAggregator.GetEvent<OpenHabDisconnected>().Subscribe(OnDisconnected, ThreadOption.UIThread, false);

            if (!_connectionTracker.IsConnected)
            {
                IsConnecting = true;
                _connectionTracker.CheckConnectionAsync();
            }
            else
            {
                ReloadPage();
            }
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);

            if (_loadingCancellationTokenSource != null)
            {
                _loadingCancellationTokenSource.Cancel();
                _loadingCancellationTokenSource = null;
            }

            _eventAggregator.GetEvent<SettingsChangedEvent>().Unsubscribe(OnSettingsChanged);
            _eventAggregator.GetEvent<OpenHabConnected>().Unsubscribe(OnConnected);
            _eventAggregator.GetEvent<OpenHabDisconnected>().Unsubscribe(OnDisconnected);
        }

        private void OnConnected(OpenHabConnectedPayload e)
        {
            _promptService.ShowNotification(e.Message, "");

            IsConnecting = false;
            ReloadPage();
        }

        private void OnDisconnected(OpenHabDisconnectedPayload e)
        {
            IsConnecting = false;

            _promptService.ShowError("Disconnected", e.Message,
                new[]
                {
                    new UICommand("Close"),
                    new UICommand("Settings", c => OpenSettingsPage())
                });
        }

        private void OnSettingsChanged(Settings settings)
        {
            //LoadPage();
        }

        private void ReloadPage()
        {
            _loadingCancellationTokenSource = new CancellationTokenSource();

            // it was already loaded
            if (_currentPage != null)
            {
                LoadPage(_currentPage, false);
            }
            else // loading first time
            {
                if (IsHomepage)
                {
                    // this is Homepage so load sitemap list first
                    LoadSitemaps(true);
                }
                else
                {
                    // it's regular page
                    LoadPage(_parameters.Page, true);
                }
            }
        }

        private void LoadSitemaps(bool autoSelect)
        {
            var settings = _settingsManager.CurrentSettings;
            if (settings == null)
            {
                Log.Warn("Settings not set");
                return;
            }

            Task.Run(() =>
            {
                return _connectionTracker.Execute(async client =>
                    (await client.GetSitemaps(_loadingCancellationTokenSource.Token)).ToArray());
            }, _loadingCancellationTokenSource.Token)
                .ContinueWith(
                    t =>
                    {
                        Sitemap[] allSitemaps = t.Result;

                        if (allSitemaps.Length == 0)
                        {
                            _promptService.ShowError("No sitemap", "openHAB doesn't have any sitemaps configured",
                                null);
                            return;
                        }

                        if (autoSelect)
                        {
                            var sitemap = allSitemaps.FirstOrDefault(s => s.Name == settings.Sitemap);
                            if (sitemap != null)
                            {
                                // Found configured sitemap 
                                LoadPage(sitemap.Homepage, true);
                                return;
                            }

                            if (allSitemaps.Length == 1)
                            {
                                // Only one sitemap defined so use it
                                sitemap = allSitemaps[0];
                                LoadPage(sitemap.Homepage, true);
                                return;
                            }
                        }


                        // Need to show sitemap selection screen first
                        Sitemaps = allSitemaps;
                        SelectedSitemap = null;
                        ShowSitemapDialog = true;

                    }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void LoadPage(Page page, bool reset)
        {
            _currentPage = page;

            Log.Debug("Loading page {0} from {1}", page.Id, page.Link);

            if (reset)
            {
                PageTitle = null;
                Frames = null;
            }

            IsLoading = true;
            Task.Run(() =>
            {
                return _connectionTracker.Execute(client=> client.GetPage(page, _loadingCancellationTokenSource.Token));

            }, _loadingCancellationTokenSource.Token)
            .ContinueWith(t =>
            {
                IsLoading = false;
                LastUpdateTime = DateTime.Now.ToString("HH:mm:ss.fff");

                if (t.IsCanceled)
                    return;
                if (t.IsFaulted)
                    return;
                
                ProcessPage(t.Result);

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ProcessPage(SitemapPage page)
        {
            PageTitle = page.Title;

            if (page.Widgets == null)
            {
                //TODO: show message that page is empty
                return;
            }

            var frameWidgets = page.Widgets.Where(w => w.Type == WidgetType.Frame).ToArray();
            var nonFrameWidgets = page.Widgets.Where(w => w.Type != WidgetType.Frame).ToArray();

            int newFrameCount = frameWidgets.Length;
            if (nonFrameWidgets.Length > 0)
                newFrameCount++;

            // check if collection has changed
            bool framesChanged = false;
            if (_frames == null || _frames.Count != newFrameCount)
            {
                framesChanged = true;
            }
            else
            {
                for (int index = 0; index < frameWidgets.Length; index++)
                {
                    if (_frames[index].WidgetId != frameWidgets[index].WidgetId)
                    {
                        framesChanged = true;
                        break;
                    }
                }
            }

            // use existing, or rebuild if has changed
            ObservableCollection<FrameWidgetViewModel> frames;
            if (_frames != null && !framesChanged) frames = _frames;
            else frames = new ObservableCollection<FrameWidgetViewModel>();

            for (int index = 0; index < frameWidgets.Length; index++)
            {
                var widget = frameWidgets[index];

                FrameWidgetViewModel frameViewModel;
                if (index < frames.Count)
                {
                    // update existing
                    frameViewModel = frames[index];
                }
                else
                {
                    // create new 
                    frameViewModel = (FrameWidgetViewModel)_widgetViewModelFactory.Create(widget);
                    frames.Insert(index, frameViewModel);
                }

                frameViewModel.Update(widget);
            }

            if (nonFrameWidgets.Length > 0)
            {
                var defaultFrame = frames.FirstOrDefault(w => w.WidgetId == DefaultFrameId);

                var defaultFrameWidget = new Widget();
                defaultFrameWidget.Type = WidgetType.Frame;
                defaultFrameWidget.Label = "";
                defaultFrameWidget.WidgetId = DefaultFrameId;
                defaultFrameWidget.Widgets = nonFrameWidgets;

                // create or update default frame
                if (defaultFrame == null)
                {
                    defaultFrame = (FrameWidgetViewModel)_widgetViewModelFactory.Create(defaultFrameWidget);
                    frames.Add(defaultFrame);
                }

                defaultFrame.Update(defaultFrameWidget);
            }

            Frames = frames;
        }

        private void OpenSettingsPage()
        {
            _navigationService.Navigate(PageToken.Settings, null);
        }

        private void OpenHomePage()
        {
            _navigationService.Navigate(PageToken.Hub, new HubPageParameters{IsHomepage = true});
        }

        private void AcceptSitemapDialog()
        {
            if (SelectedSitemap != null)
            {
                var settings = _settingsManager.CurrentSettings;
                settings.Sitemap = SelectedSitemap.Name;
                _settingsManager.SaveSettings(settings);

                LoadPage(SelectedSitemap.Homepage, true);
            }

            CloseSitemapDialog();
        }

        private void CloseSitemapDialog()
        {
            SelectedSitemap = null;
            Sitemaps = null;
            ShowSitemapDialog = false;
        }
    }
}
