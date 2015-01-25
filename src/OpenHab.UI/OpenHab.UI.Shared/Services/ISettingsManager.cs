namespace OpenHab.UI.Services
{
    public interface ISettingsManager
    {
        Settings LoadSettings();

        void SaveSettings(Settings settings);

        Settings CurrentSettings { get; }
    }
}