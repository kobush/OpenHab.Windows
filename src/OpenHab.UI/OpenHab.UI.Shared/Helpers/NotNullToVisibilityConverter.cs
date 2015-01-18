using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Coding4Fun.Toolkit.Controls.Converters;

namespace OpenHab.UI.Helpers
{
    public class NotEmptyToVisibilityConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture, string language)
        {

#if DEBUG && NETFX_CORE
            //Always return true for the designer, for easy blend support
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return Visibility.Visible;
#endif
            if (value is Visibility)
            {
                return value;
            }

            bool isVisible = true;
            if (value is bool)
            {
                isVisible = (bool) value;
            }
            else if (value is int || value is short || value is long)
            {
                isVisible = 0 != (int) value;
            }
            else if (value is float || value is double)
            {
                isVisible = 0.0 != (double) value;
            }
            else if (value is string && string.IsNullOrEmpty((string) value))
            {
                isVisible = false;
            }
            else if (value is IEnumerable<object>)
            {
                isVisible = ((IEnumerable<object>) value).Any();
            }
            else if (value is IEnumerable)
            {
                isVisible = ((IEnumerable) value).GetEnumerator().MoveNext();
            }
            else if (value == null)
            {
                isVisible = false;
            }

            if ((string) parameter == "!") //Inverse visibility
            {
                isVisible = !isVisible;
            }
            return isVisible ? Visibility.Visible : Visibility.Collapsed;

        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture, string language)
        {
            throw new NotSupportedException();
        }
    }
}
