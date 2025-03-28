using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class PlexAssetFileCatagory : IPlexCatagory
  {
    public string Catagory => "Assets";
    public IReadOnlyList<PlexAssetViewModel> Assets => assets;
    public string ParentPath => parentPath;

    private List<PlexAssetViewModel> assets;
    private string parentPath;

    public PlexAssetFileCatagory(string parentPath, IPlexAsset[] assets)
    {
      this.parentPath = parentPath;
      this.assets = new List<PlexAssetViewModel>();
      foreach (IPlexAsset asset in assets)
      {
        PlexAssetViewModel assetViewModel = new PlexAssetViewModel(asset);
        assetViewModel.TryLoad(parentPath);
        this.assets.Add(assetViewModel);
      }

    }

    public IPlexCatagory FindCatagoryForFile(PlexFile file)
    {
      return null;
    }

    public Dictionary<string, string> MoveContentTo(PlexItem item, string tempFilePath)
    {
      Dictionary<string, string> result = new Dictionary<string, string>();
      foreach (PlexAssetViewModel asset in assets)
      {
        if (string.IsNullOrEmpty(asset.FilePath)) continue;
        string extension = Path.GetFileName(asset.FilePath).Substring(Path.GetFileNameWithoutExtension(asset.FilePath).Length);
        string newName = asset.Name + extension;
        try
        {
          if (File.Exists(asset.FilePath))
          {
            File.Move(asset.FilePath, Path.Combine(tempFilePath, newName));
            result.Add(Path.Combine(tempFilePath, newName), asset.FilePath);
          }
        }
        catch
        {
          continue;
        }
      }
      return result;
    }

    public int GetFileCount()
    {
      return assets.Count(a => !string.IsNullOrEmpty(a.FilePath) && File.Exists(a.FilePath));
    }

    public IPlexCatagory FindCatagoryForFolder(PlexFolder toFind)
    {

      return null;
    }

    public void Validate(PlexItem item)
    {
      foreach (PlexAssetViewModel asset in assets)
      {
        if (File.Exists(asset.FilePath))
        {
          string extension = Path.GetFileName(asset.FilePath).Substring(Path.GetFileNameWithoutExtension(asset.FilePath).Length);
          string newName = asset.Name + extension;
          if (Path.Combine(parentPath, newName) != asset.FilePath)
          {
            item.HasWarnings = true;
          }
        }
      }
    }

    public bool HasChanges(PlexItem item)
    {
      foreach (PlexAssetViewModel asset in assets)
      {
        if (string.IsNullOrEmpty(asset.FilePath)) continue;
        string extension = Path.GetFileName(asset.FilePath).Substring(Path.GetFileNameWithoutExtension(asset.FilePath).Length);
        string newName = asset.Name + extension;
        try
        {
          if (!File.Exists(Path.Combine(item.FilePath, newName))) return true;
        }
        catch
        {
          continue;
        }
      }
      return false;
    }

    public bool ValidateChanges(PlexItem item)
    {
      HashSet<string> names = new HashSet<string>();
      foreach (PlexAssetViewModel asset in assets)
      {
        if (File.Exists(asset.FilePath))
        {
          string extension = Path.GetFileName(asset.FilePath).Substring(Path.GetFileNameWithoutExtension(asset.FilePath).Length);
          string newName = asset.Name + extension;
          if (!names.Add(newName))
          {
            return false; // we do not allow duplicate names in the folder.
          }
        }
      }
      return true;
    }
  }

}
