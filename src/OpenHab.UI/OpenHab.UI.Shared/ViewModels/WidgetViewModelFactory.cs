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

        public WidgetViewModelBase Create(Widget widget)
        {
            var widgetViewModel = CreateCore(widget);

            // this can't be set via dependency because requires actual type
            widgetViewModel.Log = _logManager.GetLogger(widget.GetType());
            return widgetViewModel;
        }

        private WidgetViewModelBase CreateCore(Widget widget)
        {
            switch (widget.Type)
            {
                case WidgetType.Frame:
                    return _container.Resolve<FrameWidgetViewModel>();
                case WidgetType.Group:
                    return _container.Resolve<GroupWidgetViewModel>();
                case WidgetType.Switch:
                    if (widget.HasMappings)
                        return new SectionSwitchWidgetViewModel();
                    
                    if (widget.Item != null && widget.Item.Type == ItemType.RollershutterItem)
                        return _container.Resolve<RollerShutterWidgetViewModel>();

                    return _container.Resolve<SwitchWidgetViewModel>();
                case WidgetType.Text:
                    return _container.Resolve<TextWidgetViewModel>();
                case WidgetType.Slider:
                    return _container.Resolve<SliderWidgetViewModel>();
                case WidgetType.Image:
                    return _container.Resolve<ImageWidgetViewModel>();
                case WidgetType.Selection:
                    return _container.Resolve<SelectionWidgetViewModel>();
                case WidgetType.Setpoint:
                    return _container.Resolve<SetpointWidgetViewModel>();
                case WidgetType.Chart:
                    return _container.Resolve<ChartWidgetViewModel>();
                case WidgetType.Video:
                    return _container.Resolve<VideoWidgetViewModel>();
                case WidgetType.WebView:
                    return _container.Resolve<WebViewWidgetViewModel>();
                case WidgetType.Colorpicker:
                    return _container.Resolve<ColorPickerWidgetViewModel>();
                case WidgetType.RollerShutter:
                    return _container.Resolve<RollerShutterWidgetViewModel>();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal class ColorPickerWidgetViewModel : WidgetViewModelBase
    {
    }

    internal class WebViewWidgetViewModel : WidgetViewModelBase
    {
    }

    public class VideoWidgetViewModel : WidgetViewModelBase
    {
    }

    public class SetpointWidgetViewModel : WidgetViewModelBase
    {
    }

    public class SelectionWidgetViewModel : WidgetViewModelBase
    {
    }

    public class ImageWidgetViewModel : WidgetViewModelBase
    {
    }
}