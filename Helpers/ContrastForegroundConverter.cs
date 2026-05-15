using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LightsOut.Helpers;

public sealed class ContrastForegroundConverter : IValueConverter
{
    private static readonly SolidColorBrush LightForeground = CreateFrozenBrush(Colors.White);
    private static readonly SolidColorBrush DarkForeground = CreateFrozenBrush(Colors.Black);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not SolidColorBrush brush)
        {
            return LightForeground;
        }

        var color = brush.Color;
        var brightness = ((0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B)) / 255d;

        return brightness >= 0.6 ? DarkForeground : LightForeground;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
