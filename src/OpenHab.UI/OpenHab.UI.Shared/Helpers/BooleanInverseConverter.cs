using System;
using System.Globalization;
using Coding4Fun.Toolkit.Controls.Converters;

namespace OpenHab.UI.Helpers
{
    public class BooleanInverseConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture, string language)
        {
            var boolValue = System.Convert.ToBoolean(value);

            return !boolValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture, string language)
        {
            var boolValue = System.Convert.ToBoolean(value);

            return !boolValue;
        }
    }
}