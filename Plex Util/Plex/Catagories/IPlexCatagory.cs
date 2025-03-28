using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public interface IPlexCatagory
  {
    public string Catagory { get; }
    public IPlexCatagory FindCatagoryForFile(PlexFile file);
    Dictionary<string, string> MoveContentTo(PlexItem item, string tempFilePath);
    public int GetFileCount();
    IPlexCatagory FindCatagoryForFolder(PlexFolder folder);
    void Validate(PlexItem item);
    bool HasChanges(PlexItem item);
    bool ValidateChanges(PlexItem item);
  }
}
