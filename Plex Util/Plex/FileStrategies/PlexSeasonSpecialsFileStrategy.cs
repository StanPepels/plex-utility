using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class PlexSeasonSpecialsFileStrategy : IPlexFileStrategy
  {
    public PlexSeasonSpecialsFileStrategy()
    {
    }
    public List<IPlexFileProperty> CreateFileProperties()
    {
      return new List<IPlexFileProperty> { new PlexEpisodeNumberFileProperty(), new PlexEpisodeTitleFileProperty() };
    }

    public string GetFileName(PlexItem item, PlexFile file)
    {
      return $"{item.DisplayName} - s{0.ToString("D2")}";
    }
  }
}
