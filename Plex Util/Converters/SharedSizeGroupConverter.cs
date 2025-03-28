using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Plex_Util
{
  public class SharedSizeGroupConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string path && parameter is string prefix)
      {
        return Regex.Replace($"{prefix}_{value}", @"\W", "_");
      }
      return value; // Fallback in case of incorrect data
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException(); // Only one-way binding
    }
  }
}