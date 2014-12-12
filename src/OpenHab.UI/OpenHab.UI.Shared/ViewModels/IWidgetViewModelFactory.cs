using OpenHab.Client;

namespace OpenHab.UI.ViewModels
{
    public interface IWidgetViewModelFactory
    {
        WidgetViewModelBase Create(WidgetType type);
    }
}