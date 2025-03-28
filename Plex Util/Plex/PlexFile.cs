using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Plex_Util
{
  public class PlexFile : INotifyPropertyChanged
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
          OnPropertyChanged(nameof(FileName));
          OnPropertyChanged(nameof(Parent));
          OnPropertyChanged(nameof(Duration));
        }
      }
    }

    public string Duration => FileProperties.GetMediaDuration(FilePath);
    public string Parent => Path.GetDirectoryName(FilePath);
    public string FileName => Path.GetFileName(FilePath);
    public IReadOnlyList<IPlexFileProperty> Properties => properties;
    private List<IPlexFileProperty> properties;
    private string filePath;

    public event PropertyChangedEventHandler PropertyChanged;

    public PlexFile(string filePath, List<IPlexFileProperty> properties)
    {
      this.filePath = filePath;
      this.properties = properties;
    }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
