using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class TitleInfo : INotifyPropertyChanged
  {
    public string Name
    {
      get => name;
      set
      {
        if (name != value)
        {
          name = value;
          OnPropertyChanged(nameof(Name));
        }
      }
    }

    public string Length
    {
      get => length;
      set
      {
        if (length != value)
        {
          length = value;
          OnPropertyChanged(nameof(Length));
        }
      }
    }


    public long Size
    {
      get => size;
      set
      {
        if (size != value)
        {
          size = value;
          OnPropertyChanged(nameof(Size));
          OnPropertyChanged(nameof(SizeText));
        }
      }
    }
    public string SizeText
    {
      get => MakeMKVItem.GetReadableFileSize(Size);
    }

    private string length;
    private string name;
    private long size;
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
