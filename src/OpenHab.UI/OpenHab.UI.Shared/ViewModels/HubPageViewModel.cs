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
        public Page Page { get; set; }
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
        private ObservableCollection<FrameWidgetViewModel> _frames;
        private CancellationTokenSource _loadingCancellationTokenSource;
        private HubPageParameters _parameters;
        private bool _isLoading;
        private string _lastUpdateTime;
        private bool _isHomepage;
        private const string DefaultFrameId = "__default_frame__";

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

        public ObservableCollection<FrameWidgetViewModel> Frames
        {
            get { return _frames; }
            private set { SetProperty(ref _frames, value); }
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

                Page page = null;
                if (IsHomepage)
                {
                    // load home page
                    var allSitempas = (await client.GetSitemaps(_loadingCancellationTokenSource.Token)).ToArray();
                    var sitemap = allSitempas.FirstOrDefault(s => s.Name == settings.Sitemap) ??
                                  allSitempas.FirstOrDefault();

                    page = await client.GetPage(sitemap.Homepage, _loadingCancellationTokenSource.Token);
                }
                else
                {
                    // load sub-page
                    page = await client.GetPage(_parameters.Page, _loadingCancellationTokenSource.Token);
                }
                
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
                    frameViewModel = (FrameWidgetViewModel)_widgetViewModelFactory.Create(widget.Type);
                    frames.Insert(index, frameViewModel);
                }

                frameViewModel.Update(widget);
            }

            if (nonFrameWidgets.Length > 0)
            {
                var defaultFrame = frames.FirstOrDefault(w => w.WidgetId == DefaultFrameId);

                // create or update default frame
                if (defaultFrame == null)
                {
                    defaultFrame = (FrameWidgetViewModel)_widgetViewModelFactory.Create(WidgetType.Frame);
                    frames.Add(defaultFrame);
                }

                var widget = new Widget();
                widget.Type = WidgetType.Frame;
                widget.Label = "";
                widget.WidgetId = DefaultFrameId;
                widget.Widgets = nonFrameWidgets;
                defaultFrame.Update(widget);
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
    }
}
