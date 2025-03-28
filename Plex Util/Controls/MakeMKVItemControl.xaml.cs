using System;
using System.Windows;
using System.Windows.Controls;

namespace Plex_Util
{

  /// <summary>
  /// Interaction logic for MakeMKVItemControl.xaml
  /// </summary>
  public partial class MakeMKVItemControl : UserControl
  {
    public event Action<object> ItemRemoveClicked;
    public event Action<object> ItemModifiedClicked;

    public MakeMKVItemControl()
    {
      InitializeComponent();

    }

    private void RemoveItemClick(object sender, RoutedEventArgs e)
    {
      Button button = (Button)e.Source;
      ItemRemoveClicked?.Invoke((MakeMKVItem)button.DataContext);
      
    }

    private void CopyItemClick(object sender, RoutedEventArgs e)
    {
      Button button = (Button)e.Source;
      Clipboard.SetText(((MakeMKVItem)button.DataContext).FilePath);
    }

    private void ModifyItemClicked(object sender, RoutedEventArgs e)
    {
      Button button = (Button)e.Source;
      EditMakeMKVItemWindow window = new EditMakeMKVItemWindow((MakeMKVItem)button.DataContext);
      window.Owner = Application.Current.MainWindow;
      window.ShowDialog();
      ItemModifiedClicked?.Invoke((MakeMKVItem)button.DataContext);
    }
  }
}
