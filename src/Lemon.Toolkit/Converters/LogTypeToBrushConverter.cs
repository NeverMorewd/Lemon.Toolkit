using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Lemon.HandyLib.Logging.Definitions;
using System;
using System.Globalization;

namespace Lemon.Toolkit.Converters
{
    public class LogTypeToBrushConverter : IValueConverter
    {
        private IBrush? _cachedLogBrush;
        private IBrush? _cachedConsoleInBrush;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (_cachedLogBrush == null)
            {
                if (Application.Current?.Resources.TryGetResource("RetroRegularForegroundBrush", null, out var retroForegroundBrush) == true
                    && retroForegroundBrush is IBrush brush)
                {
                    _cachedLogBrush = brush;

                }
            }
            if (_cachedConsoleInBrush == null)
            {
                if (Application.Current?.Resources.TryGetResource("RetroInputForegroundBrush", null, out var retroForegroundBrush) == true
                    && retroForegroundBrush is IBrush brush)
                {
                    _cachedConsoleInBrush = brush;

                }
            }
            if (value is LogEntryType type)
            {
                switch (type)
                {
                    case LogEntryType.Log:
                    case LogEntryType.ConsoleOut:
                        return _cachedLogBrush;
                    case LogEntryType.ConsoleIn:
                        return _cachedConsoleInBrush;
                }
            }
            // 默认返回值
            return Brushes.Lime;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 此转换器仅用于单向绑定，不支持反向转换
            throw new NotImplementedException();
        }
    }
}