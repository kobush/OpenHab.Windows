using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.Mvvm;
using OpenHab.UI.Services;

namespace OpenHab.UI.ViewModels
{
    public class SettingsPageViewModel : ViewModel
    {
        private readonly ISettingsManager _settingsManager;

        public SettingsPageViewModel(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
        }
        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);
        }
    }
}
