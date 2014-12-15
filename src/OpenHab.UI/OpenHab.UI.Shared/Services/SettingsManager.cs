using System;
using Windows.Security.Credentials;
using Windows.Storage;

namespace OpenHab.UI.Services
{
    public interface ISettingsManager
    {
        Settings LoadSettings();
        void SaveSettings(Settings settings);
    }

    public class SettingsManager : ISettingsManager
    {
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
        private readonly PasswordVault _passwordVault = new PasswordVault();
        
        private const string DefaultContainerKey = "defaultSettings";

        public SettingsManager()
        {
            
        }

        public Settings LoadSettings()
        {
            try
            {
                _localSettings.CreateContainer(DefaultContainerKey, ApplicationDataCreateDisposition.Existing);
                if (!_localSettings.Containers.ContainsKey(DefaultContainerKey))
                {
                    return null;
                }

                var settings = new Settings();
                var container = _localSettings.Containers[DefaultContainerKey];

                settings.Hostname = (string) container.Values["hostname"];
                settings.PortNumber = (int) container.Values["port"];
                settings.Sitemap = (string) container.Values["sitemap"];
                settings.UseHttps = (bool) container.Values["useHttps"];
                settings.Username = (string) container.Values["username"];

                if (!string.IsNullOrEmpty(settings.Username))
                {

                }

                return settings;
            }
            catch (Exception ex)
            {
                //TODO:
            }
            return null;
        }

        public void SaveSettings(Settings settings)
        {
            try
            {
                var container = _localSettings.CreateContainer(DefaultContainerKey,
                    ApplicationDataCreateDisposition.Always);

                container.Values["hostname"] = settings.Hostname;
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
                        //TODO: handle
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: log
            }
        }

    }
}