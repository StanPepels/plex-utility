using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class PlexMovieFileStrategy : IPlexFileStrategy
  {
    public List<IPlexFileProperty> CreateFileProperties()
    {
      return new List<IPlexFileProperty> { new PlexMovieEditionFileProperty() };
    }

    public string GetFileName(PlexItem item, PlexFile file)
    {
      return item.DisplayName;
    }
  }

}
