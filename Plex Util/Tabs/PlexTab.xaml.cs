using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
  /// Interaction logic for PlexTab.xaml
  /// </summary>
  public partial class PlexTab : TabItem, INotifyPropertyChanged
  {
    public string PlexInput
    {
      get => plexInput;
      set
      {
        if (plexInput != value)
        {
          plexInput = value;
          OnPropertyChanged(nameof(PlexInput));
        }
      }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    public ObservableCollection<PlexItem> plexItems;
    private string plexInput;

    public PlexTab()
    {
      plexItems = new ObservableCollection<PlexItem>();
      InitializeComponent();
      DataContext = this;
      plexList.ItemsSource = plexItems;
      App.OnDependenciesUpdated += HandleDependenciesUpdatedEvent;
      HandleDependenciesUpdatedEvent();
    }

    private void HandleDependenciesUpdatedEvent()
    {
      openWinScpButton.IsEnabled = App.WinScpSupported;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void BrowsePlexInputFolderClick(object sender, RoutedEventArgs e)
    {
      if (FileOpenDialog.SelectFolder(out string input, "Select inout folder", PlexInput))
      {
        PlexInput = input;
      }
    }

    private void HandleScanButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      if(string.IsNullOrEmpty(plexInput) || !Directory.Exists(plexInput))
      {
        MessageBox.Show(string.IsNullOrEmpty(plexInput) ? "No input folder specified." : $"Folder {plexInput} not found.", "Invalid input folder", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      plexItems.Clear();
      foreach (string directory in Directory.GetDirectories(plexInput))
      {
        PlexItem item = new PlexItem(directory);
        PlexFolder folder = PlexFolder.BuildFolderStructure(directory);
        folder.Validate(item);
        plexItems.Add(item);
      }

      UpdatePlexVisibleItems();
    }

    private void HandleSaveAllButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      int changesCount = plexItems.Count(item => item.Dirty && !item.HasWarnings);
      int warningCount = plexItems.Count(item => item.Dirty && item.HasWarnings);
      if (warningCount > 0)
      {
        MessageBoxResult result = MessageBox.Show("Some items have warnings the changes to these items won't be applied?", "File structure warnings", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
      }

      if (changesCount > 0)
      {
        MessageBoxResult result = MessageBox.Show($"This will move files for {changesCount} items. This might take a long time. Do you wish to apply all the changes to all modified items?", "Apply Modifications", MessageBoxButton.YesNo, MessageBoxImage.Information);
        if (result != MessageBoxResult.Yes) return;
      }

      plexProgressBar.Maximum = plexItems.Count;
      plexProgressBar.Minimum = 0;
      plexProgressBar.Value = 0;
      plexProgressBar.Text = $"(0 / {plexItems.Count})";
      plexProgressBar.Visibility = Visibility.Visible;
      Task.Run(() =>
      {
        int updatedCount = 0;
        // we can just loop over all items. changes won't be applied if it is not marked as dirty
        for (int i = 0; i < plexItems.Count; i++)
        {
          Dispatcher.Invoke(() =>
          {
            plexProgressBar.Value = i;
            plexProgressBar.Text = $"({i} / {plexItems.Count})";
          });
          if (plexItems[i].Save())
          {
            updatedCount++;
          }
        }
        Dispatcher.Invoke(() =>
        {
          plexProgressBar.Visibility = Visibility.Collapsed;
          UpdatePlexVisibleItems();
          MessageBox.Show($"Sucessfully updated {updatedCount} items", "Update Done", MessageBoxButton.OK, MessageBoxImage.Information);
        });
      });
    }

    private void SearchFocusLost(object sender, RoutedEventArgs e)
    {

      UpdatePlexVisibleItems();
    }

    private void SearchKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Return)
      {
        UpdatePlexVisibleItems();
      }
    }

    private void UpdatePlexVisibleItems()
    {
      ICollectionView view = CollectionViewSource.GetDefaultView(plexItems);

      view.Filter = item =>
      {
        PlexItem plexItem = (PlexItem)item;
        string search = plexSearchInput.Text;
        int tagDivIndex = search.IndexOf(':');
        string tag = (tagDivIndex != -1 ? search.Substring(0, tagDivIndex) : string.Empty).ToLower();
        string tagValue = (tagDivIndex != -1 ? search.Substring(tagDivIndex + 1) : string.Empty).Trim();
        switch (tag)
        {
          case "year":
            return plexItem.Year.IndexOf(tagValue, StringComparison.OrdinalIgnoreCase) != -1;
          case "imdb":
            return plexItem.Imdb.IndexOf(tagValue, StringComparison.OrdinalIgnoreCase) != -1;
          case "title":
            return plexItem.Title.IndexOf(tagValue, StringComparison.OrdinalIgnoreCase) != -1;
          default:
            return plexItem.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1;
        }
      };

      view.SortDescriptions.Clear(); // Clear existing sort orders
      view.SortDescriptions.Add(new SortDescription(nameof(PlexItem.FilePath), ListSortDirection.Ascending)); // Sort by Name in ascending order

      // Refresh the view to apply changes
      view.Refresh();
    }

    private void SortByTitleClick(object sender, RoutedEventArgs e)
    {
      plexSortYear.IsChecked = false;
      plexSortImdb.IsChecked = false;
      plexSortWarnings.IsChecked = false;
      ICollectionView view = CollectionViewSource.GetDefaultView(plexItems);
      view.SortDescriptions.Clear(); // Clear existing sort orders
      view.SortDescriptions.Add(new SortDescription(plexSortTitle.IsChecked.HasValue && plexSortTitle.IsChecked.Value ? nameof(PlexItem.Title) : nameof(PlexItem.FilePath), ListSortDirection.Ascending)); // Sort by Name in ascending order
      view.Refresh();
    }

    private void SortByYearClick(object sender, RoutedEventArgs e)
    {

      plexSortTitle.IsChecked = false;
      plexSortImdb.IsChecked = false;
      plexSortWarnings.IsChecked = false;
      ICollectionView view = CollectionViewSource.GetDefaultView(plexItems);
      view.SortDescriptions.Clear(); // Clear existing sort orders
      view.SortDescriptions.Add(new SortDescription(plexSortYear.IsChecked.HasValue && plexSortYear.IsChecked.Value ? nameof(PlexItem.Year) : nameof(PlexItem.FilePath), ListSortDirection.Ascending)); // Sort by Name in ascending order
      view.Refresh();
    }

    private void SortByImdbTagClick(object sender, RoutedEventArgs e)
    {
      plexSortTitle.IsChecked = false;
      plexSortYear.IsChecked = false;
      plexSortWarnings.IsChecked = false;
      ICollectionView view = CollectionViewSource.GetDefaultView(plexItems);
      view.SortDescriptions.Clear(); // Clear existing sort orders
      view.SortDescriptions.Add(new SortDescription(plexSortImdb.IsChecked.HasValue && plexSortImdb.IsChecked.Value ? nameof(PlexItem.Imdb) : nameof(PlexItem.FilePath), ListSortDirection.Ascending)); // Sort by Name in ascending order
      view.Refresh();

    }

    private void SortByWarningTagClick(object sender, RoutedEventArgs e)
    {
      plexSortTitle.IsChecked = false;
      plexSortYear.IsChecked = false;
      plexSortImdb.IsChecked = false;
      ICollectionView view = CollectionViewSource.GetDefaultView(plexItems);
      view.SortDescriptions.Clear(); // Clear existing sort orders
      view.SortDescriptions.Add(new SortDescription(plexSortWarnings.IsChecked.HasValue && plexSortWarnings.IsChecked.Value ? nameof(PlexItem.HasWarnings) : nameof(PlexItem.FilePath), ListSortDirection.Descending)); // Sort by Name in ascending order
      view.Refresh();

    }

    private void HandleOpenWinScpButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      ProcessStartInfo makeMkvProcess = new ProcessStartInfo();
      makeMkvProcess.FileName = App.WinScpPath;
      makeMkvProcess.Arguments = $"";
      makeMkvProcess.UseShellExecute = false;
      makeMkvProcess.CreateNoWindow = true;
      Process.Start(makeMkvProcess);
    }

    private void HandleOpenLogButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      App.OpenLog();
    }
  }
}
