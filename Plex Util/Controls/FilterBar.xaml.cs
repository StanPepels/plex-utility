using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Plex_Util.FilterBar;

namespace Plex_Util
{
  /// <summary>
  /// Interaction logic for FilterBar.xaml
  /// </summary>
  public partial class FilterBar : UserControl, INotifyPropertyChanged
  {
    public class Filter
    {
      public string filterProperty { get; set; }
      public string Name { get; set; }
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(IEnumerable), typeof(ProgressControl));
    public IEnumerable Source
    {
      get => (IEnumerable)GetValue(SourceProperty);
      set
      {
        SetValue(SourceProperty, value);
      }
    }

    public string SelectedButton
    {
      get => selectedButton;
      set
      {
        if (selectedButton != value)
        {
          selectedButton = value;
          OnPropertyChanged(nameof(SelectedButton));
        }
      }
    }

    private string selectedButton;
    private Filter defaultSort;
    public event PropertyChangedEventHandler PropertyChanged;
    public FilterBar()
    {
      this.DataContext = this;
      InitializeComponent();
    }

    public void SetFilters(Filter defaultSort, params Filter[] filters)
    {
      items.ItemsSource = filters;
      this.defaultSort = defaultSort;
    }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void HandleButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      ToggleButton button = (ToggleButton)sender;
      Filter filter = defaultSort;
      if (button.IsChecked.HasValue && button.IsChecked.Value)
      {
        filter = button.DataContext as Filter;
      }
      SelectedButton = filter.Name;
      ICollectionView view = CollectionViewSource.GetDefaultView(Source);
      view.SortDescriptions.Clear(); // Clear existing sort orders
      view.SortDescriptions.Add(new SortDescription(filter.filterProperty, ListSortDirection.Descending)); // Sort by Name in ascending order
      view.Refresh();
    }

    public void ApplyDefaultFilter()
    {
      Filter filter = defaultSort;
      SelectedButton = filter.Name;
      ICollectionView view = CollectionViewSource.GetDefaultView(Source);
      view.SortDescriptions.Clear(); // Clear existing sort orders
      view.SortDescriptions.Add(new SortDescription(filter.filterProperty, ListSortDirection.Descending)); // Sort by Name in ascending order
      view.Refresh();
      selectedButton = null;
    }
  }
}
