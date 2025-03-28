using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public interface IPlexFileProperty : INotifyPropertyChanged
  {
    public string Name { get; }
    public bool IsValidValue { get; }
    public string DecoratePlexName(string suggestedName);
    public void ForFile(string file);
  }

  public interface IPlexFileProperty<T> : IPlexFileProperty, INotifyPropertyChanged
  {
    public T Value { get; set; }
  }
}
