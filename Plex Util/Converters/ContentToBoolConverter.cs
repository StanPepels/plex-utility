﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Plex_Util
{
  public class ContentToBoolConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length < 2 || values[0] == null || values[1] == null) return false;

      // values[0] is the bound value, values[1] is the ToggleButton content
      return values[0].ToString() == values[1].ToString();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      return null;
    }
  }

}
