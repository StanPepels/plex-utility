using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace Plex_Util
{
  public class PlexItem : INotifyPropertyChanged
  {

    public string FilePath
    {
      get => filePath;
      set
      {
        if (filePath != value)
        {
          filePath = value;
          Folder = PlexFolder.BuildFolderStructure(filePath);
          OnPropertyChanged(nameof(FilePath));
          OnPropertyChanged(nameof(Name));
        }
      }
    }

    public string Title
    {
      get => title;
      set
      {
        if (title != value)
        {
          title = value;
          OnPropertyChanged(nameof(Title));
        }
      }
    }

    public string Year
    {
      get => year;
      set
      {
        if (year != value)
        {
          year = value;
          OnPropertyChanged(nameof(Year));
        }
      }
    }

    public string Imdb
    {
      get => imdb;
      set
      {
        if (imdb != value)
        {
          imdb = value;
          OnPropertyChanged(nameof(Imdb));
        }
      }
    }
    public bool HasWarnings
    {
      get => hasWarnings;
      set
      {
        if (hasWarnings != value)
        {
          hasWarnings = value;
          OnPropertyChanged(nameof(HasWarnings));
        }
      }
    }

    public bool Dirty
    {
      get => dirty;
      set
      {
        if (dirty != value)
        {
          dirty = value;
          OnPropertyChanged(nameof(Dirty));
        }
      }
    }

    public string Name => Path.GetFileName(FilePath);
    public PlexFolder Folder { get; private set; }
    public string DisplayName => $"{title} ({year})";

    private string filePath;
    private string title;
    private string year;
    private string imdb;
    private bool dirty;
    private bool hasWarnings;

    private const string imdbPattern = @"\{imdb-(?<id>[^\}]+)\}";
    private const string yearPattern = @"\((?<year>\d{4})\)";
    private const string namePattern = @"^(?<name>.+?)\s\(";
    public event PropertyChangedEventHandler PropertyChanged;

    public PlexItem(string source)
    {
      filePath = source;
      Load();
    }

    private void UpdateDirtyFlag()
    {
      string oldName = Path.GetFileName(FilePath);
      string newName = $"{DisplayName} {{imdb-{Imdb}}}";
      if (oldName != newName)
      {
        MarkDirty();
      }
    }

    internal bool Save()
    {
      if (Dirty && !hasWarnings)
      {
        // first apply the folder chnages

        string oldName = Path.GetFileName(FilePath);
        string newName = $"{DisplayName} {{imdb-{Imdb}}}";
        string stageingDir = Path.Combine(filePath + " - staging");


        Dictionary<string, string> movedFiles = Folder.MoveFiles(this, stageingDir);
        if (Directory.GetFiles(FilePath, "*", SearchOption.AllDirectories).Length == 0)
        {
          Directory.CreateDirectory(stageingDir);
          Directory.Delete(FilePath, true);
          filePath = oldName != newName ? Path.Combine(Path.GetDirectoryName(filePath), newName) : filePath;
          Directory.Move(stageingDir, filePath);
        }
        else
        {
          MessageBoxResult result = MessageBox.Show($"Error occured while trying to apply file settings: one ore more files were not moved. Would you like to retry?", "Exception", MessageBoxButton.YesNo, MessageBoxImage.Error);
          return false;
        }

        string removeImdbPattern = @"\{imdb-[^\}]+\}";
        foreach (string file in Directory.GetFiles(FilePath, "*", SearchOption.AllDirectories))
        {
          string dir = Path.GetDirectoryName(file);
          string name = Path.GetFileNameWithoutExtension(file);
          string ext = Path.GetFileName(file).Substring(Path.GetFileNameWithoutExtension(file).Length);
          string newFileName = Regex.Replace(name, removeImdbPattern, "").Trim() + ext;
          if (newFileName != Path.GetFileName(file))
          {
            File.Move(file, Path.Combine(dir, newFileName));
          }
        }

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(FilePath));
        Dirty = false;
        Folder = PlexFolder.BuildFolderStructure(filePath);
        Folder.Validate(this);
        return true;
      }
      return false;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      UpdateDirtyFlag();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private void Load()
    {
      string folderName = Path.GetFileName(filePath);

      Regex nameRegex = new Regex(namePattern);
      Match nameMatch = nameRegex.Match(folderName);

      if (nameMatch.Success)
      {
        title = nameMatch.Groups["name"].Value.Trim();
      }
      else
      {
        title = folderName.Trim();
      }

      // Match for year
      Regex yearRegex = new Regex(yearPattern);
      Match yearMatch = yearRegex.Match(folderName);

      if (yearMatch.Success)
      {
        year = yearMatch.Groups["year"].Value;
      }

      // Match for IMDb ID
      Regex imdbRegex = new Regex(imdbPattern);
      Match imdbMatch = imdbRegex.Match(folderName);

      if (imdbMatch.Success)
      {
        imdb = imdbMatch.Groups["id"].Value;
      }

      if (string.IsNullOrEmpty(imdb))
      {
        foreach (string file in Directory.GetFiles(FilePath, "*", SearchOption.AllDirectories))
        {
          imdbMatch = imdbRegex.Match(file);

          if (imdbMatch.Success)
          {
            imdb = imdbMatch.Groups["id"].Value;
            break;
          }
        }
      }
      Folder = PlexFolder.BuildFolderStructure(filePath);
    }

    internal void MarkDirty()
    {
      Dirty = true;
    }
  }
}
