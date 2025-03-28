using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class PlexPosterAsset : IPlexAsset
  {
    public string Name => "poster";
    public string FileName => Name;
  }
}
