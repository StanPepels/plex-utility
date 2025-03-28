using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Plex_Util
{
  public class ItemAndViewModelToCheckedConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 2 && values[0] is TitleInfo item && values[1] is EditMakeMKVItemWindow viewModel)
      {
        return viewModel.Item.IsTitleSelected(item);
      }
      return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

}
