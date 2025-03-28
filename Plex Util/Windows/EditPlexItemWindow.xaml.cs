using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Plex_Util
{

  /// <summary>
  /// Interaction logic for EditPlexItemWindow.xaml
  /// </summary>
  public partial class EditPlexItemWindow : Window
  {
    private struct FileMoveOperation
    {
      public string source;
      public string destination;
    }

    private const double ScrollThreshold = 10; // How close the mouse needs to be to the edge for scrolling
    private const double ScrollAmount = 5; // Amount to scroll when near the edge
    private const int ExpandTimeMs = 500;

    public List<PlexFolder> FolderCollection => new List<PlexFolder>() { item.Folder };
    private PlexFile draggedFile;
    private PlexItem item;
    private ScrollViewer scrollViewer;
    private Timer expandTimer;
    private TreeViewItem hoveredItem;
    private HashSet<int> vlcProcesses;

    public EditPlexItemWindow(PlexItem item)
    {
      this.DataContext = this;
      this.item = item;
      this.vlcProcesses = new HashSet<int>();
      expandTimer = new Timer(ExpandTimeMs);
      expandTimer.AutoReset = true;
      expandTimer.Elapsed += (sender, args) =>
      {
        Dispatcher.Invoke(() =>
        {
          if (hoveredItem != null && !hoveredItem.IsExpanded)
          {
            hoveredItem.IsExpanded = true;
          }
        });
      };
      InitializeComponent();
    }
    protected override void OnClosing(CancelEventArgs e)
    {
      if (vlcProcesses.Count > 0)
      {
        e.Cancel = MessageBox.Show($"Some vlc instances are still open would you like to close these?", "open vlc instances", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No;
        if (!e.Cancel)
        {
          foreach(int processId in vlcProcesses)
          {
            Process process =  Process.GetProcessById(processId);
            process.Kill();
            process.WaitForExit();
          }
        }
      }
    }

    private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var source = e.Source as DependencyObject;

      // The DataContext is inherited, so we can directly access it from the source element
      var dataContext = GetDataContext(source);

      if (dataContext != null)
      {
        draggedFile = dataContext as PlexFile;
        scrollViewer = FindVisualChild<ScrollViewer>(treeView);
        Task scrollingTask = Task.Run(async () =>
        {
          while (draggedFile != null)
          {
            await Task.Delay(5);
            Dispatcher.Invoke(() =>
            {
              GetCursorPosition(out int x, out int y);
              ScrollIfNeeded(y);
            });
          }
        });
        DragDrop.DoDragDrop((UIElement)source, draggedFile, DragDropEffects.Move);
        draggedFile = null;
        expandTimer.Stop();
      }


    }

    private void StackPanel_DragOver(object sender, DragEventArgs e)
    {
      e.Effects = DragDropEffects.Move;

      // Check if the drop target is a valid TreeViewItem
      if (e.OriginalSource is DependencyObject source)
      {
        TreeViewItem targetItem = FindAncestor<TreeViewItem>(source);
        if (targetItem != null)
        {
          // Visual feedback on valid drop target (optional)
          targetItem.Background = Brushes.LightGray; // Highlight the target item
        }
      }

      e.Handled = true;  // Mark the event as handled
    }

    private void StackPanel_Drop(object sender, DragEventArgs e)
    {
      if (e.OriginalSource is DependencyObject source)
      {
        TreeViewItem targetItem = FindAncestor<TreeViewItem>(source);
        if (targetItem != null)
        {
          // Remove highlight after drop
          targetItem.Background = Brushes.Transparent; // Reset the background
          PlexFileCatagory targetCatagory = targetItem.DataContext as PlexFileCatagory;
          PlexFileCatagory sourceCatagory = item.Folder.FindFileCatagory(draggedFile) as PlexFileCatagory;
          PlexUncatagorizedFileCatagory uncatgorizedCatagory = item.Folder.FindFileCatagory(draggedFile) as PlexUncatagorizedFileCatagory;
          if ((sourceCatagory == null && uncatgorizedCatagory == null) || targetCatagory == null)
          {
            return;
          }
          if (sourceCatagory != null && sourceCatagory.Files.Remove(draggedFile) || uncatgorizedCatagory.Files.Remove(draggedFile))
          {
            targetCatagory.AddFile(draggedFile);
            targetItem.IsExpanded = true;
          }
        }
      }
      draggedFile = null;
    }

    private static object GetDataContext(DependencyObject element)
    {
      while (element != null)
      {
        if (element is FrameworkElement frameworkElement)
        {
          return frameworkElement.DataContext;
        }
        element = VisualTreeHelper.GetParent(element);
      }
      return null;
    }

    private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
      while (current != null && !(current is T))
      {
        current = VisualTreeHelper.GetParent(current);
      }
      return current as T;
    }

    private void StackPanel_DragLeave(object sender, DragEventArgs e)
    {
      if (e.OriginalSource is DependencyObject source)
      {
        TreeViewItem targetItem = FindAncestor<TreeViewItem>(source);
        if (targetItem != null)
        {
          // Remove highlight after drop
          targetItem.Background = Brushes.Transparent; // Reset the background
        }
      }
    }


    private void ScrollIfNeeded(double mouseY)
    {
      double scrollableHeight = scrollViewer.ExtentHeight - scrollViewer.ViewportHeight;
      double scrollPosition = scrollViewer.VerticalOffset;

      // Scroll down if near the bottom edge
      if (mouseY >= scrollViewer.ViewportHeight - ScrollThreshold)
      {
        double newPosition = Math.Min(scrollPosition + ScrollAmount, scrollableHeight);
        scrollViewer.ScrollToVerticalOffset(newPosition);
      }
      // Scroll up if near the top edge
      else if (mouseY <= ScrollThreshold)
      {
        double newPosition = Math.Max(scrollPosition - ScrollAmount, 0);
        scrollViewer.ScrollToVerticalOffset(newPosition);
      }
    }

    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
      T foundChild = null;

      // Check if parent is a valid element
      if (parent != null)
      {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
          DependencyObject child = VisualTreeHelper.GetChild(parent, i);

          // If the child is the desired type, return it
          if (child is T)
          {
            foundChild = (T)child;
            break;
          }
          else
          {
            // Recur into the child tree
            foundChild = FindVisualChild<T>(child);
            if (foundChild != null)
              break;
          }
        }
      }

      return foundChild;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
      public int X;
      public int Y;

      public static implicit operator Point(POINT point)
      {
        return new Point(point.X, point.Y);
      }
    }

    [DllImport("user32.dll")]
    static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    /// <summary>
    /// Retrieves the cursor's position, in screen coordinates.
    /// </summary>
    /// <see>See MSDN documentation for further information.</see>
    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    public void GetCursorPosition(out int x, out int y)
    {
      POINT lpPoint;
      GetCursorPos(out lpPoint);
      ScreenToClient(new WindowInteropHelper(this).Handle, ref lpPoint);
      x = lpPoint.X;
      y = lpPoint.Y;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      FrameworkElement element = e.Source as FrameworkElement;
      if (element != null)
      {
        PlexAssetViewModel asset = (PlexAssetViewModel)element.DataContext;
        if (FileOpenDialog.SelectFile(out string file, "Select file", item.FilePath))
        {
          asset.FilePath = file;
        }
      }
    }
    private void OpenFileClick(object sender, RoutedEventArgs e)
    {
      PlexFile file = ((Button)sender).DataContext as PlexFile;
      ProcessStartInfo vlcStartInfo = new ProcessStartInfo()
      {
        UseShellExecute = false,
        FileName = App.VlcPath,
        Arguments = $"\"{file.FilePath}\"",
      };
      Process vlc = new Process();
      vlc.StartInfo = vlcStartInfo;
      vlc.EnableRaisingEvents = true;
      vlc.Exited += (evt, sender) => vlcProcesses.Remove(vlc.Id);
      vlc.Start();
      vlcProcesses.Add(vlc.Id);
    }

    private void DeleteClick(object sender, RoutedEventArgs e)
    {
      PlexFile file = ((Button)sender).DataContext as PlexFile;
      if (file != null)
      {
        IPlexCatagory catagory = item.Folder.FindFileCatagory(file);
        switch (catagory)
        {
          case PlexFileCatagory fileCatagory:
            fileCatagory.RemoveFile(file);
            break;
          case PlexUncatagorizedFileCatagory uncatagorizedFileCatagory:
            uncatagorizedFileCatagory.RemoveFile(file);
            break;
        }
      }
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
      PlexFolderCatagory folder = ((Button)sender).DataContext as PlexFolderCatagory;
      if (folder != null)
      {
        folder.AddFolder();
      }
    }

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {

      PlexFolder folder = ((Button)sender).DataContext as PlexFolder;
      if (folder != null)
      {
        PlexFolderCatagory catagory = this.item.Folder.FindFolderCatagory(folder) as PlexFolderCatagory;
        if (catagory != null)
        {
          catagory.TryRemoveFolder(folder);
        }
      }
    }

    private void StackPanel_DragEnter(object sender, DragEventArgs e)
    {

      e.Effects = DragDropEffects.None;
      if (e.OriginalSource is DependencyObject source)
      {
        TreeViewItem targetItem = FindAncestor<TreeViewItem>(source);
        hoveredItem = targetItem;
        expandTimer.Start();
      }
      e.Handled = true;
    }

    private void StackPanel_DragLeave_1(object sender, DragEventArgs e)
    {
      e.Effects = DragDropEffects.Move;
      expandTimer.Stop();
      hoveredItem.Background = Brushes.Transparent; // Highlight the target item
      e.Handled = true;
    }

    private void StackPanel_DragOver_1(object sender, DragEventArgs e)
    {
      e.Effects = DragDropEffects.None;
      hoveredItem.Background = Brushes.LightGray; // Highlight the target item
      e.Handled = true;
    }
  }
}
