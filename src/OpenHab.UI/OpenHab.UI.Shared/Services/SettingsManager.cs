using System;
using Windows.Security.Credentials;
using Windows.Storage;
using MetroLog;
using Microsoft.Practices.Prism.PubSubEvents;

namespace OpenHab.UI.Services
{
    public class SettingsManager : ISettingsManager
    {
        private readonly ILogger Log;

        private readonly IEventAggregator _eventAggregator;
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
        private readonly PasswordVault _passwordVault = new PasswordVault();
        
        private Settings _currentSettings;

        private const string DefaultContainerKey = "defaultSettings";

        public SettingsManager(ILogManager logManager, IEventAggregator eventAggregator)
        {
            Log = logManager.GetLogger<SettingsManager>();
            _eventAggregator = eventAggregator;
        }

        public Settings CurrentSettings 
        {
            get
            {
                return (_currentSettings) ?? (_currentSettings = LoadSettings());
            }
        }

        public Settings LoadSettings()
        {
            try
            {
                if (!_localSettings.Containers.ContainsKey(DefaultContainerKey))
                {
                    Log.Debug("Settings container not found");
                    return null;
                }

                _localSettings.CreateContainer(DefaultContainerKey, ApplicationDataCreateDisposition.Existing);
                var container = _localSettings.Containers[DefaultContainerKey];

                var settings = new Settings();
                settings.DemoMode = (bool)container.Values["demoMode"];
                settings.Hostname = (string)container.Values["hostname"];
                settings.RemoteHostname = (string)container.Values["altHostname"];
                settings.PortNumber = (int) container.Values["port"];
                settings.Sitemap = (string) container.Values["sitemap"];
                settings.UseHttps = (bool) container.Values["useHttps"];
                settings.Username = (string) container.Values["username"];

                if (!string.IsNullOrEmpty(settings.Username))
                {
                    var credential = _passwordVault.Retrieve("openhab", settings.Username);
                    if (credential != null)
                    {
                        credential.RetrievePassword();
                        settings.Password = credential.Password;
                    }
                }

                Log.Debug("Loaded settings");

                return settings;
            }
            catch (Exception ex)
            {
                Log.Error("Error loading settings", ex);
            }
            return null;
        }

        public void SaveSettings(Settings settings)
        {
            try
            {
                var container = _localSettings.CreateContainer(DefaultContainerKey, ApplicationDataCreateDisposition.Always);

                container.Values["demoMode"] = settings.DemoMode;
                container.Values["hostname"] = settings.Hostname;
                container.Values["altHostname"] = settings.RemoteHostname;
                container.Values["port"] = settings.PortNumber;
                container.Values["sitemap"] = settings.Sitemap;
                container.Values["useHttps"] = settings.UseHttps;
                container.Values["username"] = settings.Username;

                if (!string.IsNullOrEmpty(settings.Username) && !string.IsNullOrEmpty(settings.Password))
                {
                    try
                    {
                        _passwordVault.Add(new PasswordCredential("openhab", settings.Username, settings.Password));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error saving password", ex);
                        return;
                    }
                }

                Log.Debug("Saved settings");

                _currentSettings = settings;

                _eventAggregator.GetEvent<SettingsChangedEvent>().Publish(settings);
            }
            catch (Exception ex)
            {
                Log.Error("Error saving settings", ex);
            }
        }

    }

    public class SettingsChangedEvent : PubSubEvent<Settings>
    { }
}