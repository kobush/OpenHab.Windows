using System;
using Microsoft.Practices.Unity;
using OpenHab.Client;

namespace OpenHab.UI.ViewModels
{
    public class WidgetViewModelFactory : IWidgetViewModelFactory
    {
        private readonly IUnityContainer _container;

        public WidgetViewModelFactory(IUnityContainer container)
        {
            _container = container;
        }

        public WidgetViewModelBase Create(WidgetType widgetType, ItemType itemType)
        {
            switch (widgetType)
            {
                case WidgetType.Frame:
                    return _container.Resolve<FrameWidgetViewModel>();
                case WidgetType.Group:
                    return _container.Resolve<GroupWidgetViewModel>();
                case WidgetType.Text:
                    return _container.Resolve<TextWidgetViewModel>();
                case WidgetType.Switch:
                    if (itemType == ItemType.RollershutterItem)
                        return _container.Resolve<RollerShutterWidgetViewModel>();
                    if (itemType == ItemType.SwitchItem)
                        return _container.Resolve<SwitchWidgetViewModel>();
                    
                    throw new ArgumentOutOfRangeException();

                case WidgetType.RollerShutter:
                    return _container.Resolve<RollerShutterWidgetViewModel>();
                case WidgetType.Chart:
                    return _container.Resolve<ChartWidgetViewModel>();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class RollerShutterWidgetViewModel : WidgetViewModelBase
    {
    }
}