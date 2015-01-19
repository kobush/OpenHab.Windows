using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Windows.UI;

namespace OpenHab.UI.Helpers
{
    public static class ColorConverter
    {
        public static Color? Parse(string color)
        {
            if (color.StartsWith("#"))
            {
                var offset = 1;

                byte a, r, g, b;
                if (color.Length == 7)
                {
                    //#RRGGBB
                    a = 255;
                    r = Byte.Parse(color.Substring(0 + offset, 2), NumberStyles.HexNumber);
                    g = Byte.Parse(color.Substring(2 + offset, 2), NumberStyles.HexNumber);
                    b = Byte.Parse(color.Substring(4 + offset, 2), NumberStyles.HexNumber);
                }
                else // length= 9
                {
                    //#AARRGGBB
                    a = Byte.Parse(color.Substring(0 + offset, 2), NumberStyles.HexNumber);
                    r = Byte.Parse(color.Substring(2 + offset, 2), NumberStyles.HexNumber);
                    g = Byte.Parse(color.Substring(4 + offset, 2), NumberStyles.HexNumber);
                    b = Byte.Parse(color.Substring(6 + offset, 2), NumberStyles.HexNumber);
                }

                return Color.FromArgb(a, r, g, b);
            }

            // try to parse from name
            var property = typeof (Colors).GetRuntimeProperties().FirstOrDefault(f => string.Equals(f.Name, color, StringComparison.OrdinalIgnoreCase));
            if (property != null)
                return (Color) property.GetValue(null);

            return null;
        }
    }
}