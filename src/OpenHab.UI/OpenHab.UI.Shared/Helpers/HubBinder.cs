using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OpenHab.UI.Helpers
{
    public class HubBinder : DependencyObject
    {
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.RegisterAttached(
            "HeaderTemplate",
            typeof(DataTemplate),
            typeof(HubBinder), new PropertyMetadata(null, HeaderTemplateChanged)
            );

        private static void HeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var hub = d as Hub;
            if (hub == null) return;
            var template = e.NewValue as DataTemplate;
            if (template == null) return;
            foreach (var hubSection in hub.Sections)
            {
                hubSection.HeaderTemplate = template;
            }
        }
        public static void SetHeaderTemplate(UIElement element, DataTemplate value)
        {
            element.SetValue(HeaderTemplateProperty, value);
        }

        public static DataTemplate GetHeaderTemplate(UIElement element)
        {
            return element.GetValue(HeaderTemplateProperty) as DataTemplate;
        }

        public static readonly DependencyProperty SectionTemplateProperty = DependencyProperty.RegisterAttached(
            "SectionTemplate",
            typeof(DataTemplate),
            typeof(HubBinder), new PropertyMetadata(null, SectionTemplateChanged)
            );

        private static void SectionTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var hub = d as Hub;
            if (hub == null) return;
            var template = e.NewValue as DataTemplate;
            if (template == null) return;
            foreach (var hubSection in hub.Sections)
            {
                hubSection.ContentTemplate = template;
            }
        }
        public static void SetSectionTemplate(UIElement element, DataTemplate value)
        {
            element.SetValue(SectionTemplateProperty, value);
        }

        public static DataTemplate GetSectionTemplate(UIElement element)
        {
            return element.GetValue(SectionTemplateProperty) as DataTemplate;
        }
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.RegisterAttached(
            "DataSource",
            typeof(object),
            typeof(HubBinder), new PropertyMetadata(null, DataSourceChanged)
            );

        private static void DataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var data = e.NewValue as IEnumerable;
            var hub = d as Hub;

            if (data == null || hub == null) return;
            hub.Sections.Clear();

            var template = GetSectionTemplate(hub);
            var header = GetHeaderTemplate(hub);
            foreach (var section in data)
            {
                var sect = new HubSection { DataContext = section, ContentTemplate = template, HeaderTemplate = header };
                var hubData = section as IHubData;
                if (hubData != null)
                {
                    sect.Header = hubData.Header;
                }

                hub.Sections.Add(sect);
            }
        }

        public static void SetDataSource(UIElement element, object value)
        {
            element.SetValue(DataSourceProperty, value);
        }

        public static object GetDataSource(UIElement element)
        {
            return element.GetValue(DataSourceProperty);
        }
    }

    public interface IHubData
    {
        object Header { get; }
    }
}
