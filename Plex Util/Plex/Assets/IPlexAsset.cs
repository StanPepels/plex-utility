using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public interface IPlexAsset
  {
    string Name { get; }
    string FileName { get; }
  }
}
