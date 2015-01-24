using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Prism.Mvvm;

namespace OpenHab.UI.Views
{
    public sealed partial class HubPage : IView
    {
        public HubPage()
        {
            InitializeComponent();

            //TODO: quick workaround - use theme color instead
            StatusBar.GetForCurrentView().ForegroundColor = Colors.Black;
        }
    }
}
