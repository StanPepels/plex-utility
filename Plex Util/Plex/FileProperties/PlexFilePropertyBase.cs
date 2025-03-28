using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public abstract class PlexFilePropertyBase<T> : IPlexFileProperty<T>
  {
    public T Value
    {
      get
      {
        return value;
      }
      set
      {
        if (this.value == null || !this.value.Equals(value))
        {
          this.value = value;
          OnPropertyChanged(nameof(Value));
          OnPropertyChanged(nameof(IsValidValue));
        }
      }
    }


    public abstract string Name { get; }

    public bool IsValidValue
    {
      get => CheckValue();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private T value;

    public abstract string DecoratePlexName(string suggestedName);
    public abstract void ForFile(string filePath);
    public abstract bool CheckValue();

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
