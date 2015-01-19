using System;
using MetroLog;
using Microsoft.Practices.Unity;
using OpenHab.Client;

namespace OpenHab.UI.ViewModels
{
    public class WidgetViewModelFactory : IWidgetViewModelFactory
    {
        private readonly IUnityContainer _container;
        private readonly ILogManager _logManager;

        public WidgetViewModelFactory(IUnityContainer container, ILogManager logManager)
        {
            _container = container;
            _logManager = logManager;
        }

        public WidgetViewModelBase Create(WidgetType widgetType, ItemType itemType)
        {
            var widget = CreateCore(widgetType, itemType);

            // this can't be set via dependency because requires actual type
            widget.Log = _logManager.GetLogger(widget.GetType());
            return widget;
        }

        private WidgetViewModelBase CreateCore(WidgetType widgetType, ItemType itemType)
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
                    
                    return _container.Resolve<SwitchWidgetViewModel>();

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