using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class PlexAssetViewModel : INotifyPropertyChanged
  {
    private IPlexAsset asset;
    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => asset.Name;
    public string FilePath
    {
      get
      {
        return filePath;
      }
      set
      {
        if (filePath != value)
        {
          filePath = value;
          OnPropertyChanged(nameof(filePath));

        }
      }
    }

    private string filePath;

    public PlexAssetViewModel(IPlexAsset asset)
    {
      this.asset = asset;
    }

    public bool TryLoad(string directory)
    {
      if (Directory.Exists(directory))
      {
        foreach (string file in Directory.GetFiles(directory))
        {
          if (Path.GetFileNameWithoutExtension(file) == asset.FileName)
          {
            FilePath = file;
            return true;
          }
        }
      }

      return false;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  
}
