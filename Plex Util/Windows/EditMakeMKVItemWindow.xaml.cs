using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Plex_Util
{
 
  /// <summary>
  /// Interaction logic for EditMakeMKVItemWindow.xaml
  /// </summary>
  public partial class EditMakeMKVItemWindow : Window
  {
    public MakeMKVItem Item { get; set; }
    public ICommand CheckBoxCommand { get; set; }
    public EditMakeMKVItemWindow(MakeMKVItem item)
    {
      this.Item = item;
      CheckBoxCommand = new RelayCommand(OnCheckBoxChanged);
      this.DataContext = this;
      InitializeComponent();
    }
    private void OnCheckBoxChanged(object parameter)
    {
      if (parameter is TitleInfo item)
      {
        Item.ToggleTitle(item);
      }
    }
  }
}
