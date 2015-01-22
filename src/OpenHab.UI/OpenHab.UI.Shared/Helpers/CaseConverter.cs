using System;
using System.Globalization;
using Coding4Fun.Toolkit.Controls.Converters;

namespace OpenHab.UI.Helpers
{
    public class CaseConverter : ValueConverter
    {
        public bool ToLower { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture, string language)
        {
            string text = value as string;
            if (text != null)
                return (ToLower) ? text.ToLower() : text.ToUpper();

            return "";
        }
    }
}