using System;
using Windows.UI.Xaml.Data;

namespace OpenHab.UI.Helpers
{
    public class CaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string text = value as string;
            if (text != null)
            {
                return text.ToUpper();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}