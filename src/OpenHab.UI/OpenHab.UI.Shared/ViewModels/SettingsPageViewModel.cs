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
using OpenHab.Client;
using OpenHab.UI.Services;
using Zeroconf;

namespace OpenHab.UI.ViewModels
{
    public class SettingsPageViewModel : ViewModel
    {
        private readonly ISettingsManager _settingsManager;
        private readonly INavigationService _navigationService;

        private DelegateCommand _testConnectionCommand;
        private DelegateCommand _saveSettingsCommand;
        private DelegateCommand _discoverServerCommand;
        private DelegateCommand _discoverCancelCommand;
        private DelegateCommand _discoverAcceptCommand;

        private Settings _lastSettings;
        
        private string _hostname;
        private string _portNumber;
        private bool _useHttps;
        private bool _useAuthentication;
        private string _username;
        private string _password;
        private string _sitemap;
        private IEnumerable<Sitemap> _sitemaps;

        private CancellationTokenSource _testConnectionCancellationTokenSource;
        private bool _isConnecting;
        private string _connectionMessage;

        private CancellationTokenSource _discoverCancellationTokenSource;
        private string _discoverMessage;
        private bool _isSearching;
        private bool _showDiscoverPopup;
        private IEnumerable<IZeroconfHost> _discoveredServers;


        public SettingsPageViewModel(ISettingsManager settingsManager, INavigationService navigationService)
        {
            _settingsManager = settingsManager;
            _navigationService = navigationService;
        }

        public ICommand TestConnectionCommand
        {
            get { return (_testConnectionCommand) ?? (_testConnectionCommand = new DelegateCommand(TestConnection)); }
        }

        public ICommand SaveSettingsCommand
        {
            get { return (_saveSettingsCommand) ?? (_saveSettingsCommand = new DelegateCommand(SaveSettings)); }
        }

        public ICommand DiscoverServerCommand
        {
            get { return (_discoverServerCommand) ?? (_discoverServerCommand = new DelegateCommand(DiscoverServer)); }
        }

        public ICommand DiscoverCancelCommand
        {
            get { return (_discoverCancelCommand) ?? (_discoverCancelCommand = new DelegateCommand(CloseDiscoverDialog)); }
        }
        public ICommand DiscoverAcceptCommand
        {
            get { return (_discoverAcceptCommand) ?? (_discoverAcceptCommand = new DelegateCommand(AcceptDiscoverServer)); }
        }

        public string Hostname
        {
            get { return _hostname; }
            set { SetProperty(ref _hostname, value); }
        }

        public string PortNumber
        {
            get { return _portNumber; }
            set { SetProperty(ref _portNumber, value); }
        }

        public bool UseHttps
        {
            get { return _useHttps; }
            set { SetProperty(ref _useHttps, value); }
        }

        public bool UseAuthentication
        {
            get { return _useAuthentication; }
            set { SetProperty(ref _useAuthentication, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public string Sitemap
        {
            get { return _sitemap; }
            set { SetProperty(ref _sitemap, value); }
        }

        public IEnumerable<Sitemap> Sitemaps
        {
            get { return _sitemaps; }
            private set { SetProperty(ref _sitemaps, value); }
        }

        public bool IsConnecting
        {
            get { return _isConnecting; }
            private set { SetProperty(ref _isConnecting, value); }
        }

        public string ConnectionMessage
        {
            get { return _connectionMessage; }
            private set { SetProperty(ref _connectionMessage, value); }
        }

        public string DiscoverMessage
        {
            get { return _discoverMessage; }
            private set { SetProperty(ref _discoverMessage, value); }
        }

        public bool IsSearching
        {
            get { return _isSearching; }
            private set { SetProperty(ref _isSearching, value); }
        }

        public bool ShowDiscoverPopup
        {
            get { return _showDiscoverPopup; }
            set { SetProperty(ref _showDiscoverPopup, value); }
        }

        public IEnumerable<IZeroconfHost> DiscoveredServers
        {
            get { return _discoveredServers; }
            private set { SetProperty(ref _discoveredServers, value); }
        }

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

            // load previously saved settings
            _lastSettings = _settingsManager.LoadSettings();
            if (_lastSettings != null)
            {
                Hostname = _lastSettings.Hostname;
                PortNumber = _lastSettings.PortNumber != null ? _lastSettings.PortNumber.ToString() : "";
                UseHttps = _lastSettings.UseHttps;

                UseAuthentication = (!string.IsNullOrEmpty(_lastSettings.Username));
                Username = _lastSettings.Username;
                Password = _lastSettings.Password; //TODO: change to random string?!?

                Sitemap = _lastSettings.Sitemap;
            }
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);

            // cancel connection
            if (_testConnectionCancellationTokenSource != null)
            {
                _testConnectionCancellationTokenSource.Cancel();
                _testConnectionCancellationTokenSource = null;
            }

            if (_discoverCancellationTokenSource != null)
            {
                _discoverCancellationTokenSource.Cancel();
                _discoverCancellationTokenSource = null;
            }
        }


        private Settings GetSettings()
        {
            var settings = new Settings();
            settings.Hostname = Hostname;
            settings.PortNumber = !string.IsNullOrEmpty(PortNumber) ? Convert.ToInt32(PortNumber) : (int?) null;
            settings.UseHttps = UseHttps;
            return settings;
        }

        private void TestConnection()
        {
            if (string.IsNullOrEmpty(Hostname))
                return; //todo: show validation message

            var settings = GetSettings();
            var baseUri = settings.ResolveBaseUri();

            var client = new OpenHabRestClient(baseUri);

            _testConnectionCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            IsConnecting = true;
            ConnectionMessage = "Connecting...";
            Task.Run(() => client.GetSitemaps(_testConnectionCancellationTokenSource.Token))
                .ContinueWith(t =>
                {
                    IsConnecting = false;

                    if (t.IsCanceled) 
                        return;

                    if (t.IsFaulted)
                    {
                        ConnectionMessage = "Connection error";
                        return;
                    }

                    ConnectionMessage = "Success!";
                    Sitemaps = new ObservableCollection<Sitemap>(t.Result);
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void SaveSettings()
        {
            var settings = GetSettings();

            _settingsManager.SaveSettings(settings);

            //TODO: broadcast settings changed

            if (_navigationService.CanGoBack())
                _navigationService.GoBack();
            else
                _navigationService.Navigate("Hub", new HubPageParameters {IsHomepage = true});
        }



        private void DiscoverServer()
        {
            CloseDiscoverDialog();

            const string httpProtocol = "_openhab-server._tcp.local";
            const string httpsProtocol = "_openhab-server-ssl._tcp.local";

            ShowDiscoverPopup = true;
            IsSearching = true;
            DiscoverMessage = "Searching for openHAB services, please wait ...";

            _discoverCancellationTokenSource = new CancellationTokenSource();

            // find services
            Task.Run(() => ZeroconfResolver.ResolveAsync(httpProtocol, 
                    scanTime: TimeSpan.FromSeconds(5),
                    cancellationToken: _discoverCancellationTokenSource.Token))
                .ContinueWith(t =>
                {
                    IsSearching = false;

                    if (t.IsCanceled)
                        return;

                    if (t.IsFaulted)
                        return;

                    if (t.Result.Count == 0)
                    {
                        DiscoverMessage = "None services found. Make sure UDP port 5353 is not blocked by server's firewall.";
                    }
                    else
                    {
                        DiscoverMessage = string.Format("Found {0} service(s).", t.Result.Count);

                        DiscoveredServers = t.Result;
                    }

                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CloseDiscoverDialog()
        {
            if (_discoverCancellationTokenSource != null)
            {
                _discoverCancellationTokenSource.Cancel();
                _discoverCancellationTokenSource = null;
            }

            ShowDiscoverPopup = false;
        }

        private void AcceptDiscoverServer()
        {
            //TODO: get selected server
            

            CloseDiscoverDialog();
        }

    }
}
