using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OpenHab.UI.ViewModels;

namespace OpenHab.UI
{
    public class WidgetTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return base.SelectTemplateCore(item);
        }

        public DataTemplate FrameWidgetTemplate { get; set; }

        public DataTemplate GroupWidgetTemplate { get; set; }

        public DataTemplate TextWidgetTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is GroupWidgetViewModel)
                return GroupWidgetTemplate;

            if (item is FrameWidgetViewModel)
                return FrameWidgetTemplate;

            if (item is TextWidgetViewModel)
                return TextWidgetTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}