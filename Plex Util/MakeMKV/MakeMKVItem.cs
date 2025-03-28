using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class MakeMKVItem : INotifyPropertyChanged
  {
    public string FilePath
    {
      get => filePath;
      set
      {
        if (filePath != value)
        {
          filePath = value;
          OnPropertyChanged(nameof(FilePath));
        }
      }
    }
    public string StatusText
    {
      get => statusText;
      set
      {
        if (statusText != value)
        {
          statusText = value;
          OnPropertyChanged(nameof(StatusText));
          OnPropertyChanged(nameof(ShowRemoveButton));
        }
      }
    }
    public int CurrentProgress
    {
      get => currentProgress;
      set
      {
        if (currentProgress != value)
        {
          currentProgress = value;
        }
      }
    }

    public int TotalProgress
    {
      get => totalProgress;
      set
      {
        if (totalProgress != value)
        {
          totalProgress = value;
        }
      }
    }

    public int ProgressMaxValue
    {
      get => progressMaxValue;
      set
      {
        if (progressMaxValue != value)
        {
          progressMaxValue = value;
          HasProgress = value > 0;
        }
      }
    }

    public bool HasProgress
    {
      get => hasProgress;
      set
      {
        if (hasProgress != value)
        {
          hasProgress = value;
        }
      }
    }


    public string Error
    {
      get => error;
      set
      {
        if (error != value)
        {
          error = value;
          OnPropertyChanged(nameof(Error));
        }
      }
    }

    public int TitleCount
    {
      get => Titles.Count;
    }


    public string CurrentProgressText
    {
      get => $"{(int)(((double)currentProgress / (double)progressMaxValue) * 100)}%";
    }
    public string TotalProgressText
    {
      get => $"{(int)(((double)totalProgress / (double)progressMaxValue) * 100)}%";
    }

    public bool ShowRemoveButton
    {
      get => string.IsNullOrEmpty(statusText);
    }

    public string SizeText
    {
      get => GetReadableFileSize(Size);
    }

    public string TitleCountText
    {
      get => $"(Titles: {SelectedTitleCountText} / {Titles.Count})";
    }

    public string SelectedTitleCountText
    {
      get => $"{selectedIndices.Count}";
    }

    public long Size
    {
      get => size;
    }

    public ObservableCollection<TitleInfo> Titles { get; set; }

    private string statusText;
    private string filePath;
    private int currentProgress;
    private int totalProgress;
    private int progressMaxValue;
    private bool hasProgress;
    private long size;
    private string error;
    private HashSet<int> selectedIndices;

    public event PropertyChangedEventHandler PropertyChanged;

    public MakeMKVItem()
    {
      this.Error = string.Empty;
      Titles = new ObservableCollection<TitleInfo>();
      selectedIndices = new HashSet<int>();
    }


    public void NotifyProgressChanged()
    {
      OnPropertyChanged(nameof(ProgressMaxValue));
      OnPropertyChanged(nameof(TotalProgress));
      OnPropertyChanged(nameof(TotalProgressText));
      OnPropertyChanged(nameof(CurrentProgress));
      OnPropertyChanged(nameof(CurrentProgressText));
      OnPropertyChanged(nameof(HasProgress));
    }

    public void UpdateTitleInfo()
    {
      size = 0;
      foreach (int index in selectedIndices)
      {
        size += Titles[index].Size;
      }
      OnPropertyChanged(nameof(TitleCount));
      OnPropertyChanged(nameof(TitleCountText));
      OnPropertyChanged(nameof(Size));
      OnPropertyChanged(nameof(SizeText));
      OnPropertyChanged(nameof(SelectedTitleCountText));
    }

    public void SelectAllTitles()
    {
      for (int i = 0; i < Titles.Count; i++)
      {
        selectedIndices.Add(i);
      }
      UpdateTitleInfo();
    }

    public void DeselectTitle(int index)
    {
      if (selectedIndices.Remove(index)) UpdateTitleInfo();
    }

    public void SelectTitle(int index)
    {
      if (selectedIndices.Add(index)) UpdateTitleInfo();
    }

    public void DeselectAllTitles()
    {
      selectedIndices.Clear();
      UpdateTitleInfo();
    }

    public void ToggleTitle(TitleInfo info)
    {
      int index = Titles.IndexOf(info);
      if (selectedIndices.Contains(index)) DeselectTitle(index);
      else SelectTitle(index);
    }
    public bool IsTitleSelected(TitleInfo info)
    {
      return selectedIndices.Contains(Titles.IndexOf(info));
    }

    public List<TitleInfo> GetSelectedTitles()
    {
      return selectedIndices.Select(index => Titles[index]).ToList();
    }
    public void ResetProgress()
    {
      ProgressMaxValue = 0;
      CurrentProgress = 0;
      TotalProgress = 0;
      NotifyProgressChanged();
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static string GetReadableFileSize(long byteCount)
    {
      string[] sizeSuffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB" };
      if (byteCount < 0) { return "N/A"; }
      if (byteCount == 0) { return "0 Bytes"; }

      int suffixIndex = (int)Math.Floor(Math.Log(byteCount, 1024));
      double readableValue = byteCount / Math.Pow(1024, suffixIndex);

      return $"{readableValue:0.##} {sizeSuffixes[suffixIndex]}";
    }
  }
}
