using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class PlexExtraFileStrategy : IPlexFileStrategy
  {
    public List<IPlexFileProperty> CreateFileProperties()
    {
      return new List<IPlexFileProperty>() { new PlexTitleProperty() };
    }

    public string GetFileName(PlexItem item, PlexFile file)
    {
      return string.Empty;
    }
  }
}
