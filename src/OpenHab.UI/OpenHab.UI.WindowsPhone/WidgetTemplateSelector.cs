using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OpenHab.UI.ViewModels;

namespace OpenHab.UI
{
    public class WidgetTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FrameWidgetTemplate { get; set; }

        public DataTemplate GroupWidgetTemplate { get; set; }

        public DataTemplate TextWidgetTemplate { get; set; }

        public DataTemplate SwitchWidgetTemplate { get; set; }

        public DataTemplate SliderWidgetTemplate { get; set; }

        public DataTemplate RollerShutterWidgetTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is GroupWidgetViewModel)
                return GroupWidgetTemplate;

            if (item is FrameWidgetViewModel)
                return FrameWidgetTemplate;

            if (item is TextWidgetViewModel)
                return TextWidgetTemplate;

            if (item is SwitchWidgetViewModel)
                return SwitchWidgetTemplate;

            if (item is SliderWidgetViewModel)
                return SliderWidgetTemplate;

            if (item is RollerShutterWidgetViewModel)
                return RollerShutterWidgetTemplate;

            return base.SelectTemplateCore(item, container);
        }

    }
}