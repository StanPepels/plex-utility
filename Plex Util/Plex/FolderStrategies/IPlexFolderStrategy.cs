using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public interface IPlexFolderStrategy
  {
    string FolderName { get; }

    PlexFolder CreateNew(string path, IReadOnlyList<PlexFolder> existing);
    List<PlexFolder> ListFolders(string path);
  }
}
