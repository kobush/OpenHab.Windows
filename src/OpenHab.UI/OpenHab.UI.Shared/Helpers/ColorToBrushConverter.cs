using System;
using System.Globalization;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Coding4Fun.Toolkit.Controls.Converters;

namespace OpenHab.UI.Helpers
{
    public class ColorToBrushConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture, string language)
        {
            if (value == null)
                return parameter;

            if (value is Brush)
                return value;

            if (value is Color)
                return new SolidColorBrush((Color) value);

            if (value is string)
            {
                var color = ColorConverter.Parse((string) value);
                if (color == null)
                    throw new NotSupportedException("ColorToBurshConverter can't parse color string " + value);

                return new SolidColorBrush(color.Value);
            }

            throw new NotSupportedException("ColorToBurshConverter only supports converting from Color and String");
        }
    }
}