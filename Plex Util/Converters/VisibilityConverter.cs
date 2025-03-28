using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Plex_Util
{

  public class VisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((bool)value) ? Visibility.Visible : parameter is string hideText && bool.Parse(hideText) ? Visibility.Hidden : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
