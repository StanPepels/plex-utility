using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Plex_Util
{
  /// <summary>
  /// Interaction logic for PathSelectControl.xaml
  /// </summary>
  public partial class PathSelectControl : UserControl
  {
    public static readonly DependencyProperty PathProperty = DependencyProperty.Register(nameof(Path), typeof(string), typeof(PathSelectControl));
    public string Path
    {
      get => (string)GetValue(PathProperty);
      set
      {
        SetValue(PathProperty, value);
      }
    }

    public static readonly DependencyProperty IsFileProperty = DependencyProperty.Register(nameof(IsFile), typeof(bool), typeof(PathSelectControl));
    public bool IsFile
    {
      get => (bool)GetValue(IsFileProperty);
      set
      {
        SetValue(IsFileProperty, value);
      }
    }

    public PathSelectControl()
    {
      InitializeComponent();
    }



    private void HandleBrowseButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      if (IsFile)
      {
        if (FileOpenDialog.SelectFile(out string input, "Select file", Path))
        {
          Path = input;
        }
      }
      else
      {
        if (FileOpenDialog.SelectFolder(out string input, "Select folder", Path))
        {
          Path = input;
        }
      }
    }
  }
}
