using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Plex_Util
{
  /// <summary>
  /// Interaction logic for EncodeTab.xaml
  /// </summary>
  public partial class EncodeTab : TabItem, INotifyPropertyChanged
  {
    public string Output
    {
      get => output;
      set
      {
        if (output != value)
        {
          output = value;
          OnPropertyChanged(nameof(Output));
        }
      }
    }


    public string Encode
    {
      get => encode;
      set
      {
        if (encode != value)
        {
          encode = value;
          OnPropertyChanged(nameof(Encode));
        }
      }
    }

    public string EncodePresetPath
    {
      get => encodePresetPath;
      set
      {
        if (encodePresetPath != value)
        {
          encodePresetPath = value;
          if (File.Exists(encodePresetPath))
          {
            List<string> values = new List<string>();
            JArray list = JObject.Parse(File.ReadAllText(encodePresetPath))["PresetList"] as JArray;
            if (list != null)
            {
              foreach (JObject obj in list)
              {
                values.Add(obj.GetValue("PresetName").Value<string>());
              }
            }
            presetOptions.ItemsSource = values;
            presetOptions.SelectedIndex = 0;
          }
          else
          {
            presetOptions.ItemsSource = new List<string>();
          }
          OnPropertyChanged(nameof(EncodePresetPath));
        }
      }
    }

    public string Preset
    {
      get => preset;
      set
      {
        if (preset != value)
        {
          preset = value;
          OnPropertyChanged(nameof(Preset));
        }
      }
    }

    public string Input
    {
      get => input;
      set
      {
        if (input != value)
        {
          input = value;
          OnPropertyChanged(nameof(Input));
        }
      }
    }

    public bool SkipExisingItems
    {
      get => skipExistingItems;
      set
      {
        if (skipExistingItems != value)
        {
          skipExistingItems = value;
          OnPropertyChanged(nameof(SkipExisingItems));
        }
      }
    }

    public int WhenDoneActionIndex
    {
      get => whenDoneActionIndex;
      set
      {
        if (whenDoneActionIndex != value && value >= 0 && value < whenDoneOptions.Count)
        {
          whenDoneActionIndex = value;
          OnPropertyChanged(nameof(WhenDoneActionIndex));
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public ObservableCollection<MakeMKVItem> makeMKVItems;
    private string output;
    private string input;
    private string encodePresetPath;
    private string encode;
    private string preset;
    private bool closeRequested;
    private Task processingTask;
    private CancellationTokenSource convertProcessToken;
    private bool skipExistingItems;
    private int whenDoneActionIndex;

    private List<string> whenDoneOptions = new List<string>
    {
      "Do Nothing",
      "Sleep",
      "Shutdown"
    };

    public EncodeTab()
    {
      List<string> t = new List<string>();
      makeMKVItems = new ObservableCollection<MakeMKVItem>();
      InitializeComponent();
      DataContext = this;
      processingList.ItemsSource = makeMKVItems;
      App.OnDependenciesUpdated += HandleDependenciesUpdatedEvent;
      HandleDependenciesUpdatedEvent();
      whenDoneComboBox.ItemsSource = whenDoneOptions;
    }

    private void HandleDependenciesUpdatedEvent()
    {
      openMakeMKVButton.IsEnabled = App.MakeMKVSupported;
      openHandbrakeButton.IsEnabled = App.HandbrakeSupported;
      convertButton.IsEnabled = App.MakeMKVSupported && App.HandbrakeCliSuppported;
      scanButton.IsEnabled = App.MakeMKVSupported && App.HandbrakeCliSuppported;
    }

    public bool StopRunningTask()
    {
      if (processingTask != null && !processingTask.IsCompleted)
      {
        convertProcessToken.Cancel();
        closeRequested = true;
        return true;
      }
      return false;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    private void HandleConverterScanButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      makeMKVItems.Clear();
      foreach (string file in Directory.GetFiles(input, "*.iso"))
      {
        makeMKVItems.Add(new MakeMKVItem() { FilePath = file, Type = EMakeMKVItemType.Dvd, StatusText = "(Ready for scan)" });
      }

      foreach (string directory in Directory.GetDirectories(input))
      {
        if (Directory.Exists(Path.Combine(directory, "BDMV")) || Directory.GetFiles(directory, "*.mkv", SearchOption.AllDirectories).Length > 0)
        {
          makeMKVItems.Add(new MakeMKVItem() { FilePath = directory, StatusText = "(Ready for scan)", Type = Directory.Exists(Path.Combine(directory, "BDMV")) ? EMakeMKVItemType.BluRay : EMakeMKVItemType.Mkv });
        }
      }

      Scan();
    }

    private void HandleConvertButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      DirectoryInfo outputDirectory = Directory.CreateDirectory(output);
      DirectoryInfo encodeDirectory = Directory.CreateDirectory(encode);

      closeRequested = false;
      processingTask = Task.Run(async () =>
      {
        convertProcessToken = new CancellationTokenSource();

        MakeMKVItem currentItem = null;
        try
        {
          List<MakeMKVItem> itemsToConvert = GetItemToConvert();

          if (!skipExistingItems && makeMKVItems.Any(item => Directory.Exists(GetOutputPathForItem(outputDirectory, item)) || Directory.Exists(GetEncodePathForItem(encodeDirectory, item))))
          {
            MessageBoxResult result = MessageBox.Show($"One or more output directories allready exist. These will overwritten. Any content existing content will be lost.\n Do you wich to continue?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
              return;
            }
          }
          else
          {
            int conversionCount = makeMKVItems.Count(item => skipExistingItems ? !Directory.Exists(GetOutputPathForItem(outputDirectory, item)) || !Directory.Exists(GetEncodePathForItem(encodeDirectory, item)) : true);
            MessageBoxResult result = MessageBox.Show($"This will start converting {conversionCount} items.\n Do you wish to continue?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.No)
            {
              return;
            }
          }

          foreach (MakeMKVItem item in makeMKVItems.Except(itemsToConvert)) item.StatusText = "(Skipped)";
          foreach (MakeMKVItem item in itemsToConvert)
          {
            item.StatusText = "(Queued)";
            item.ResetProgress();
          }
          Dispatcher.Invoke(() =>
          {
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0.0d;
            convertButton.IsEnabled = false;
            cancelButton.IsEnabled = true;
          });


          App.Log.WriteLine($"==================================== Started Converting ====================================");
          Exception error = null;
          try
          {
            for (int i = 0; i < itemsToConvert.Count; i++)
            {
              if (convertProcessToken.IsCancellationRequested)
              {
                for (int j = i; j < itemsToConvert.Count; j++)
                {
                  itemsToConvert[j].StatusText = "(Cancelled)";
                }
                throw new TaskCanceledException();
              }
              currentItem = itemsToConvert[i];
              currentItem.StatusText = "(Processing)";
              Dispatcher.Invoke(() =>
              {
                progressBar.Text = $"({i}/{itemsToConvert.Count})";
                progressBar.Value = (double)(i) / (double)itemsToConvert.Count;
              });

              string outputPath = GetOutputPathForItem(outputDirectory, currentItem);
              string encodePath = GetEncodePathForItem(encodeDirectory, currentItem);
              string titleFolder = await ExtractTitlesFromDisk(outputPath, currentItem, convertProcessToken);
              int[] titleIndices = await ScanDirectoryForTitles(currentItem, titleFolder);
              await EncodeTitles(currentItem, titleFolder, encodePath, titleIndices);
            }
          }
          catch (TaskCanceledException ex)
          {
            for (int j = itemsToConvert.IndexOf(currentItem); j < itemsToConvert.Count; j++)
            {
              itemsToConvert[j].StatusText = "(Cancelled)";
            }
            App.Log.WriteLine("Conversion was aborted");
            MessageBox.Show($"Conversion was aborted", "Conversion Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            if (closeRequested) Dispatcher.Invoke(Application.Current.Shutdown);
          }
          catch (Exception ex)
          {
            error = ex;
            App.Log.WriteLine(e.ToString());
          }
          finally
          {
            currentItem?.ResetProgress();
            App.Log.WriteLine($"==================================== Done Converting ====================================");
          }
          if (error != null) throw error;
          Dispatcher.Invoke(() =>
          {
            switch (whenDoneComboBox.SelectedIndex)
            {
              case 1: // sleep
                Sleep();
                break;
              case 2: // shutdown
                Shutdown();
                break;
              case 0: // Do nothing
              default:
                break;
            }
          });
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Error occured while trying to convert: {ex.Message} while converting item: {currentItem?.FilePath}", "Conversion error", MessageBoxButton.OK, MessageBoxImage.Error);
          currentItem.StatusText = "(Failed)";
          currentItem.ResetProgress();
        }
        finally
        {
          Dispatcher.Invoke(() =>
          {
            progressBar.Visibility = Visibility.Collapsed;
            convertButton.IsEnabled = true;
            cancelButton.IsEnabled = false;
            convertProcessToken.Dispose();
          });
        }
      });
    }

    private async Task<string> ExtractTitlesFromDisk(string outputPath, MakeMKVItem currentItem, CancellationTokenSource cancellationToken)
    {
      if (currentItem.Type == EMakeMKVItemType.Mkv) return currentItem.FilePath;
      if (Directory.Exists(outputPath) && !skipExistingItems)
      {
        Directory.Delete(outputPath, true);
      }
      if (!Directory.Exists(outputPath))
      {
        Directory.CreateDirectory(outputPath);
        List<TitleInfo> selected = currentItem.GetSelectedTitles();
        if (selected.Count == currentItem.TitleCount)
        {
          int exitCode = await MakeMKV.ExtractAllTitles(currentItem, outputPath, convertProcessToken, Dispatcher);
          if (cancellationToken.IsCancellationRequested)
          {
            throw new TaskCanceledException();
          }
          if (exitCode != 0)
          {
            currentItem.StatusText = "(Failed)";
            throw new Exception($"Failed to convert item {currentItem.FilePath}. Check the log for more details.");
          }
        }
        else
        {
          // slow but ok
          for (int x = 0; x < selected.Count; x++)
          {
            TitleInfo title = selected[x];
            int exitCode = await MakeMKV.ExtractTitle(currentItem, outputPath, title, convertProcessToken, Dispatcher);
            if (convertProcessToken.IsCancellationRequested)
            {
              throw new TaskCanceledException();
            }
            if (exitCode != 0)
            {
              currentItem.StatusText = "(Failed)";
              throw new Exception($"Failed to convert item {currentItem.FilePath}. Check the log for more details.");
            }
          }
        }
      }
      return outputPath;
    }

    private async Task<int[]> ScanDirectoryForTitles(MakeMKVItem currentItem, string directory)
    {
      currentItem.StatusText = "(Scanning for titles)";
      currentItem.ResetProgress();
      int[] titles;
      {
        ItemProgressUpdater progressUpdater = new ItemProgressUpdater(Dispatcher);
        progressUpdater.Start();
        currentItem.ProgressMaxValue = 100;
        currentItem.TotalProgress = 0;
        currentItem.CurrentProgress = 0;

        (int exitCode, int[] indices, TitleInfo[] _) = await Handbrake.Scan(directory, convertProcessToken, percent =>
        {
          currentItem.TotalProgress = currentItem.CurrentProgress = percent;
          progressUpdater.RequestUpdate(currentItem);
        });

        progressUpdater.Stop();
        currentItem.ResetProgress();
        currentItem.StatusText = string.Empty;
        if (exitCode != 0)
        {
          currentItem.StatusText = "(Failed)";
          throw new Exception($"Failed to scan item {currentItem.FilePath}. Check the log for more details.");
        }
        titles = indices;
      }
      return titles;
    }

    private async Task EncodeTitles(MakeMKVItem currentItem, string source, string encodePath, int[] titleIndices)
    {
      if (Directory.Exists(encodePath))
      {
        Directory.Delete(encodePath, true);
      }
      Directory.CreateDirectory(encodePath);
      for (int x = 0; x < titleIndices.Length; x++)
      {
        string name = Path.GetFileNameWithoutExtension(currentItem.FilePath);
        currentItem.StatusText = $"Encoding ({x + 1}/{titleIndices.Length})";
        currentItem.ProgressMaxValue = 100;
        int step = currentItem.ProgressMaxValue / titleIndices.Length;
        int min = x * step;

        ItemProgressUpdater progressUpdater = new ItemProgressUpdater(Dispatcher);
        progressUpdater.Start();
        void UpdateProgress(int percent)
        {
          if (percent != currentItem.CurrentProgress)
          {
            currentItem.CurrentProgress = percent;
            int total = min + currentItem.CurrentProgress / step;
            currentItem.TotalProgress = total;
            progressUpdater.RequestUpdate(currentItem);
          }
        }
        int exitCode = await Handbrake.Encode(EncodePresetPath, preset, titleIndices[x], source, Path.Combine(encodePath, $"{name}_T{x}.mkv"), convertProcessToken, UpdateProgress);
        progressUpdater.Stop();
        if (convertProcessToken.IsCancellationRequested)
        {
          throw new TaskCanceledException();
        }
        if (exitCode != 0)
        {
          currentItem.StatusText = "(Failed)";
          throw new Exception($"Failed to convert item {currentItem.FilePath}. Check the log for more details.");
        }
      }
      currentItem.StatusText = "(Done)";
      currentItem.ResetProgress();

    }
    private async Task Scan()
    {
      convertButton.IsEnabled = false;
      scanButton.IsEnabled = false;
      try
      {
        int concurrentCount = 3;
        List<Task> scans = new List<Task>();
        ConcurrentBag<MakeMKVItem> processingList = new ConcurrentBag<MakeMKVItem>(makeMKVItems);
        CancellationTokenSource scanCancellationTokenSource = new CancellationTokenSource();
        for (int i = 0; i < concurrentCount; ++i)
        {
          scans.Add(Task.Run(async () =>
          {
            while (processingList.Count > 0 && processingList.TryTake(out MakeMKVItem currentItem))
            {
              currentItem.StatusText = "(Scanning for titles)";
              if (currentItem.Type != EMakeMKVItemType.Mkv)
              {
                if ((await MakeMKV.ScanTitles(currentItem, scanCancellationTokenSource, Dispatcher) != 0))
                {
                  currentItem.Error = "Failed to scan item";
                }
                currentItem.ResetProgress();
                currentItem.StatusText = string.Empty;
              }
              else
              {
                ItemProgressUpdater progressUpdater = new ItemProgressUpdater(Dispatcher);
                progressUpdater.Start();
                currentItem.ProgressMaxValue = 100;
                currentItem.TotalProgress = 0;
                currentItem.CurrentProgress = 0;
                (int exitCode, int[] indices, TitleInfo[] titles) = await Handbrake.Scan(currentItem.FilePath, new CancellationTokenSource(), percent =>
                {
                  currentItem.TotalProgress = currentItem.CurrentProgress = percent;
                  progressUpdater.RequestUpdate(currentItem);
                });
                currentItem.Titles.Clear();
                foreach (TitleInfo titleInfo in titles) currentItem.Titles.Add(titleInfo);
                progressUpdater.Stop();
                currentItem.SelectAllTitles();
                currentItem.ResetProgress();
                currentItem.StatusText = string.Empty;
              }
            }
          }));
        }
        await Task.WhenAll(scans.ToArray());
      }
      catch (Exception e)
      {
        ShowExceptionMessage(e);
      }
      finally
      {
        convertButton.IsEnabled = !makeMKVItems.Any(item => !string.IsNullOrEmpty(item.Error));
        scanButton.IsEnabled = !makeMKVItems.Any(item => !string.IsNullOrEmpty(item.Error));
        UpdateDiskSpaceText();
      }
    }

    private void RefreshDiskSpaceButton(object sender, RoutedEventArgs e)
    {
      UpdateDiskSpaceText();
    }

    private void UpdateDiskSpaceText()
    {
      DirectoryInfo outputDirectory = Directory.CreateDirectory(output);
      DirectoryInfo rootDirectory = outputDirectory.Root;
      DriveInfo driveInfo = new DriveInfo(rootDirectory.FullName);
      long requiredDiskSpace = GetItemToConvert().Sum(item => item.Size);
      bool enoughSpace = driveInfo.AvailableFreeSpace > requiredDiskSpace;
      convertButton.IsEnabled = enoughSpace;
      diskSpaceText.Foreground = enoughSpace ? Brushes.Black : Brushes.Red;
      diskSpaceText.Text = $"{MakeMKVItem.GetReadableFileSize(requiredDiskSpace)} Required / {MakeMKVItem.GetReadableFileSize(driveInfo.AvailableFreeSpace)} Available";
    }

    private List<MakeMKVItem> GetItemToConvert()
    {
      DirectoryInfo outputDirectory = Directory.CreateDirectory(output);
      DirectoryInfo encoudeDirectory = Directory.CreateDirectory(encode);
      return makeMKVItems.Where(item =>
      {
        string outputPath = GetOutputPathForItem(outputDirectory, item);
        string encodePath = GetEncodePathForItem(encoudeDirectory, item);
        return !Directory.Exists(outputPath) || !Directory.Exists(encodePath) || !skipExistingItems;
      }).ToList();
    }

    private void HandleOpenLogButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      App.OpenLog();
    }

    private void ShowExceptionMessage(Exception e)
    {
      MessageBox.Show($"Error occured while trying to save settings: {e.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void HandleConverterCancelledButtonClickedEvent(object sender, RoutedEventArgs e)
    {
      convertProcessToken.Cancel();
    }


    private void OpenMakeMkvClick(object sender, RoutedEventArgs e)
    {
      ProcessStartInfo makeMkvProcess = new ProcessStartInfo();
      makeMkvProcess.FileName = Path.Combine(Path.GetDirectoryName(App.MakeMkvPath), "makemkv.exe");
      makeMkvProcess.Arguments = $"";
      makeMkvProcess.UseShellExecute = false;
      makeMkvProcess.CreateNoWindow = true;
      Process.Start(makeMkvProcess);

    }

    private void OpenHandbrakeClick(object sender, RoutedEventArgs e)
    {
      ProcessStartInfo makeMkvProcess = new ProcessStartInfo();
      makeMkvProcess.FileName = App.HandBrakePath;
      makeMkvProcess.Arguments = $"";
      makeMkvProcess.UseShellExecute = false;
      makeMkvProcess.CreateNoWindow = true;
      Process.Start(makeMkvProcess);
    }

    private void HandleMakeMkvItemRemovedEvent(object obj)
    {
      if (obj is MakeMKVItem toRemove)
      {
        makeMKVItems.RemoveAt(makeMKVItems.IndexOf(toRemove));
        convertButton.IsEnabled = !makeMKVItems.Any(item => !string.IsNullOrEmpty(item.Error));
        scanButton.IsEnabled = !makeMKVItems.Any(item => !string.IsNullOrEmpty(item.Error));
        UpdateDiskSpaceText();
      }
    }

    private void HandleMakeMkvItemModifiedEvent(object obj)
    {
      UpdateDiskSpaceText();
    }

    private string GetOutputPathForItem(DirectoryInfo outputDirectory, MakeMKVItem item) => Path.Combine(outputDirectory.FullName, Path.GetFileNameWithoutExtension(item.FilePath) + "_converted");
    private string GetEncodePathForItem(DirectoryInfo outputDirectory, MakeMKVItem item) => Path.Combine(outputDirectory.FullName, Path.GetFileNameWithoutExtension(item.FilePath) + "_encoded");

    private void Shutdown()
    {
      //Test.Exit();
      if (!Power.ExitWindowsEx(Windows.Win32.System.Shutdown.EXIT_WINDOWS_FLAGS.EWX_FORCE | Windows.Win32.System.Shutdown.EXIT_WINDOWS_FLAGS.EWX_SHUTDOWN | Windows.Win32.System.Shutdown.EXIT_WINDOWS_FLAGS.EWX_POWEROFF, 0))
      {
        int error = Marshal.GetLastWin32Error();
        string errorMessage = Power.GetErrorMessage(error);
        Console.WriteLine($"Failed to adjust token privileges. {errorMessage}");
        return;
      }
    }

    private void Sleep()
    {
      Power.SetSuspendState(false, true, true);
    }
  }
}
