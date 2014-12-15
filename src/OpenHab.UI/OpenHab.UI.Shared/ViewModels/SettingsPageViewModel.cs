using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using OpenHab.Client;
using OpenHab.UI.Services;

namespace OpenHab.UI.ViewModels
{
    public class SettingsPageViewModel : ViewModel
    {
        private readonly ISettingsManager _settingsManager;

        private DelegateCommand _testConnectionCommand;
        private DelegateCommand _saveSettingsCommand;

        private Settings _lastSettings;
        
        private string _hostname;
        private string _portNumber;
        private bool _useHttps;
        private bool _useAuthentication;
        private string _username;
        private string _password;
        private string _sitemap;

        private CancellationTokenSource _testConnectionCancellationTokenSource;
        private IEnumerable<Sitemap> _sitemaps;
        private bool _isConnecting;
        private string _connectionMessage;


        public SettingsPageViewModel(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public ICommand TestConnectionCommand
        {
            get { return (_testConnectionCommand) ?? (_testConnectionCommand = new DelegateCommand(TestConnection)); }
        }

        public ICommand SaveSettingsCommand
        {
            get { return (_saveSettingsCommand) ?? (_saveSettingsCommand = new DelegateCommand(SaveSettings)); }
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
                _testConnectionCancellationTokenSource.Cancel();
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
                    Sitemaps = t.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void SaveSettings()
        {
            var settings = GetSettings();

            _settingsManager.SaveSettings(settings);

            //TODO: broadcast settings changed
        }


    }
}
