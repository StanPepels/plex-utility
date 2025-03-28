using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class PlexSeasonEpisodesFileStrategy : IPlexFileStrategy
  {
    private Func<int> getIndex;
    public PlexSeasonEpisodesFileStrategy(Func<int> getIndex)
    {
      this.getIndex = getIndex;
    }
    public List<IPlexFileProperty> CreateFileProperties()
    {
      return new List<IPlexFileProperty> { new PlexEpisodeNumberFileProperty(), new PlexEpisodeTitleFileProperty() };
    }

    public string GetFileName(PlexItem item, PlexFile file)
    {
      return $"{item.DisplayName} - s{this.getIndex().ToString("D2")}";
    }
  }
}
