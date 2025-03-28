using System;
using System.Windows.Data;

namespace Plex_Util
{
  public class ValueToWidthConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values.Length != 3 || !(values[0] is double value) || !(values[1] is double maximum) || !(values[2] is double actualWidth))
        return 0;

      if (maximum <= 0)
        return 0;

      return (value / maximum) * actualWidth;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

}
