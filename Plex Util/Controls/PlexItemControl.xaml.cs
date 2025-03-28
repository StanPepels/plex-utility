using System.Windows;
using System.Windows.Controls;

namespace Plex_Util
{
  /// <summary>
  /// Interaction logic for PlexItemControl.xaml
  /// </summary>
  public partial class PlexItemControl : UserControl
  {
    public PlexItemControl()
    {
      InitializeComponent();
    }

    private void SaveItemClicked(object sender, RoutedEventArgs e)
    {
      Button button = (Button)e.Source;
      PlexItem item = (PlexItem)button.DataContext;
      if (item.HasWarnings)
      {
        MessageBoxResult result = MessageBox.Show("This item has file structure warnings do you wish to proceed?", "File structure warnings", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;
      }
      item.Save();
    }

    private void ModifyItemClicked(object sender, RoutedEventArgs e)
    {
      Button button = (Button)e.Source;
      PlexItem item = (PlexItem)button.DataContext;
      EditPlexItemWindow window = new EditPlexItemWindow(item);
      window.Owner = Application.Current.MainWindow;
      window.ShowDialog();

      if(item.Folder.HasChanged(item))
      {
        item.HasWarnings = !item.Folder.ValidateChanges(item);
        item.MarkDirty();
      }
    }
  }
}
