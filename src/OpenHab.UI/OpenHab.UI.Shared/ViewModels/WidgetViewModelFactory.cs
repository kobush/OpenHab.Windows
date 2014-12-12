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

        public WidgetViewModelBase Create(WidgetType widgetType)
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
                    return _container.Resolve<SwitchWidgetViewModel>();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}