using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using MetroLog;
using Microsoft.Practices.Prism.PubSubEvents;
using OpenHab.Client;

namespace OpenHab.UI.Services
{
    public class ConnectionTracker : IConnectionTracker
    {
        private static readonly Uri OpenHabDemoUrl = new Uri(@"https://demo.openhab.org:8443");

        private readonly ILogger Log;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsManager _settingsManager;

        private Uri _serverUrl;
        private CancellationTokenSource _connectCancellationTokenSource;

        public ConnectionTracker(
            ILogManager logManager, 
            IEventAggregator eventAggregator,
            ISettingsManager settingsManager)
        {
            Log = logManager.GetLogger<ConnectionTracker>();

            _eventAggregator = eventAggregator;
            _settingsManager = settingsManager;
        }

        public bool IsConnected
        {
            get { return _serverUrl != null; }
        }

        public Task Execute(Func<OpenHabRestClient, Task> action)
        {
            if (_serverUrl == null)
                throw new Exception("Not connected");

            var client = new OpenHabRestClient(_serverUrl);
            return action(client);
        }

        public Task<T> Execute<T>(Func<OpenHabRestClient, Task<T>> action)
        {
            if (_serverUrl == null)
                throw new Exception("Not connected");

            var client = new OpenHabRestClient(_serverUrl);
            return action(client);
        }

        public Task<bool> CheckConnectionAsync()
        {
            if (_connectCancellationTokenSource!= null)
                _connectCancellationTokenSource.Cancel();

            _connectCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            return Task.Run(async () => await CheckConnectionCore(_connectCancellationTokenSource.Token), _connectCancellationTokenSource.Token)
                .ContinueWith(
                    t =>
                    {
                        _connectCancellationTokenSource = null;

                        if (t.IsCanceled)
                            return false;

                        if (t.IsFaulted)
                        {
                            Log.Error("Error connecting to server", t.Exception);
                            return false;
                        }
                        
                        _serverUrl = t.Result;
                        return _serverUrl != null;
                    }
                );
        }

        private async Task<Uri> CheckConnectionCore(CancellationToken token)
        {
            var settings = _settingsManager.CurrentSettings;

            if (settings.UseHttps || settings.DemoMode)
            {
                InstallSslCertificate();
            }

            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile == null)
            {
                Log.Info("Network is not available");
                OnDisconnected("Network is not available");
                return null;
            }

            Uri openHabUrl;

            // if demo mode
            if (settings.DemoMode)
            {
                openHabUrl = OpenHabDemoUrl;
                Log.Info("Demo mode, url = " + openHabUrl);
                OnConnected(_serverUrl, "Running in demo mode. To connect to your openHAB please go to settings menu.");
                return openHabUrl;
            }

            // Check if we are on WiFi network
            if (connectionProfile.IsWlanConnectionProfile)
            {
                // See if we have a local URL configured in settings
                openHabUrl = settings.ResolveLocalUri();

                // If local URL is configured
                if (openHabUrl != null)
                {
                    // Check if configured local URL is reachable
                    if (await CheckUrlReachability(openHabUrl, token))
                    {
                        Log.Info("Connecting to primary URL " + openHabUrl);
                        OnConnected(openHabUrl, "Connected to primary URL");
                        return openHabUrl;
                    }
                }

                // If local URL is not reachable go with remote URL
            }

            // If we are on a mobile network go directly to remote URL from settings
            openHabUrl = settings.ResolveRemoteUrl();

            // If remote URL is configured
            if (openHabUrl != null)
            {
                if (await CheckUrlReachability(openHabUrl, token))
                {
                    Log.Info("Connecting to remote URL " + openHabUrl);
                    OnConnected(openHabUrl, "Connected to remote URL");
                    return openHabUrl;
                }
            }

            // TODO: try auto-discover 

            OnDisconnected("Please check that openHAB is running, and your settings are correct.");
            return null;
        }

        private void OnDisconnected(string message)
        {
            _eventAggregator.GetEvent<OpenHabDisconnected>()
                .Publish(new OpenHabDisconnectedPayload(message));
        }

        private void OnConnected(Uri openHabUrl, string message)
        {
            _eventAggregator.GetEvent<OpenHabConnected>()
                .Publish(new OpenHabConnectedPayload(openHabUrl, message));
        }

        private async Task<bool> CheckUrlReachability(Uri url, CancellationToken token)
         {
             Log.Debug("Checking reachability of " + url);
             
             int checkPort = url.Port;
             if (checkPort == -1)
             {
                 if (url.Scheme == "http")
                     checkPort = 80;
                 else if (url.Scheme == "https")
                     checkPort = 443;
             }

             using (var socket = new StreamSocket())
             {
                 try
                 {
                     await socket.ConnectAsync(new HostName(url.Host), checkPort.ToString()).AsTask(token);
                     Log.Debug("Socket connected");
                     return true;
                 }
                 catch (Exception ex)
                 {
                     Log.Error("Error connecting socket " + url, ex);
                     return false;
                 }
             }
         }

        public async void InstallSslCertificate() 
        {
            try
            {

                // Read the contents of the Certificate file
                var certificateFile = new Uri("ms-appx:///Assets/openhab.org.crt");

                Windows.Storage.StorageFile file =
                    await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(certificateFile);

                Windows.Storage.Streams.IBuffer certBlob = await Windows.Storage.FileIO.ReadBufferAsync(file);

                // Create an instance of the Certificate class using the retrieved certificate blob contents
                var rootCert = new Windows.Security.Cryptography.Certificates.Certificate(certBlob);

                // Get access to the TrustedRootCertificationAuthorities for your own app (not the system one)
                var trustedStore = Windows.Security.Cryptography.Certificates.CertificateStores.TrustedRootCertificationAuthorities;

                // Add the certificate to the TrustedRootCertificationAuthorities store for your app
                trustedStore.Add(rootCert);

                Log.Debug("Successfully installed SSL cert");
            }
            catch (Exception ex)
            {
                // Catch and report exceptions
                Log.Error("Exception adding cert: " + ex.Message);
            }
        }
    }

    public sealed class OpenHabConnected : PubSubEvent<OpenHabConnectedPayload>
    {}

    public sealed class OpenHabConnectedPayload
    {
        public OpenHabConnectedPayload(Uri openHabUrl, string message)
        {
            OpenHabUrl = openHabUrl;
            Message = message;
        }

        public Uri OpenHabUrl { get; private set; }

        public string Message { get; private set; }
    }

    public sealed class OpenHabDisconnected : PubSubEvent<OpenHabDisconnectedPayload>
    { }

    public sealed class OpenHabDisconnectedPayload
    {
        public OpenHabDisconnectedPayload(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
        
    }
}
