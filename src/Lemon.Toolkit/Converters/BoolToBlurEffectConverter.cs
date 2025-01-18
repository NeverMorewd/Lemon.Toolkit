using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;


namespace Lemon.Toolkit.Converters
{
    public class BoolToBlurEffectConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isBlurEnabled && isBlurEnabled)
            {
                return new BlurEffect { Radius = 15 };
            }
            return new BlurEffect { Radius = 0 };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
