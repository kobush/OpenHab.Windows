using System;
using System.Globalization;
using Coding4Fun.Toolkit.Controls.Converters;

namespace OpenHab.UI.Helpers
{
    public class StringFormatConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture, string language)
        {
            if (value == null)
                return null;

            if (parameter == null)
                return value;

            return string.Format(culture, (string)parameter, value);
        }
    }
}