using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Plex_Util
{
  public class PlexFolder
  {
    public static PlexFolder BuildFolderStructure(string path)
    {
      IPlexCatagory[] catagories = new IPlexCatagory[] {
        new PlexFileCatagory( "Movie Editions", path, new PlexMovieFileStrategy(), false),
        new PlexFileCatagory("Behind The Scenes", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Deleted Scenes", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Featurettes", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Interviews", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Scenes", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Shorts", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Trailers", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Other", path, new PlexExtraFileStrategy()),
        new PlexFolderCatagory("Seasons", path, new PlexSeasonsFolderStrategy()),
        new PlexFileCatagory( "Specials", path, new PlexSeasonSpecialsFileStrategy()),
      };

      PlexUncatagorizedFileCatagory uncatagorizedFiles = new PlexUncatagorizedFileCatagory(path, catagories);
      catagories = new List<IPlexCatagory>(catagories) { uncatagorizedFiles }.ToArray();

      IPlexAsset[] assets = new IPlexAsset[] {
        new PlexPosterAsset()
      };

      IPlexFileProperty[] properties = new IPlexFileProperty[] { };

      return new PlexFolder(path, assets, catagories, properties);
    }

    public IReadOnlyList<IPlexCatagory> Catagories => catagories;
    public IReadOnlyList<IPlexFileProperty> Properties => properties;
    public IReadOnlyList<PlexAssetViewModel> Assets => assets;
    public string LocalPath => localPath;
    public string Name => name;

    private string name;
    private string localPath;
    private List<PlexAssetViewModel> assets;
    private List<IPlexCatagory> catagories;
    private List<IPlexFileProperty> properties;

    public PlexFolder(string path, IPlexAsset[] assets, IPlexCatagory[] catagories, IPlexFileProperty[] properties)
    {
      this.name = Path.GetFileName(path);
      this.localPath = path;
      this.assets = new List<PlexAssetViewModel>();
      this.properties = new List<IPlexFileProperty>(properties);
      this.catagories = new List<IPlexCatagory>(catagories);

      if (assets.Length > 0)
      {
        this.catagories.Add(new PlexAssetFileCatagory(localPath, assets));
      }

      foreach (IPlexFileProperty property in properties)
      {
        property.ForFile(Path.Combine(localPath, name));
      }
    }

    public IPlexCatagory FindFileCatagory(PlexFile file)
    {
      foreach (IPlexCatagory catagory in catagories)
      {
        IPlexCatagory catagoryforFile = catagory.FindCatagoryForFile(file);
        if (catagoryforFile != null) return catagoryforFile;
      }
      return null;
    }

    public IPlexCatagory FindFolderCatagory(PlexFolder folder)
    {
      foreach (IPlexCatagory catagory in catagories)
      {
        IPlexCatagory catagoryforFile = catagory.FindCatagoryForFolder(folder);
        if (catagoryforFile != null) return catagoryforFile;
      }
      return null;
    }

    public int GetFileCount()
    {
      int count = 0;
      foreach (IPlexCatagory catagory in catagories)
      {
        count += catagory.GetFileCount();
      }
      return count;
    }

    internal Dictionary<string, string> MoveFiles(PlexItem item, string tempFilePath)
    {
      Dictionary<string, string> result = new Dictionary<string, string>();
      foreach (IPlexCatagory catagory in catagories)
      {
        Dictionary<string, string> catagoryMovedFiles = catagory.MoveContentTo(item, tempFilePath);
        result = result.Concat(catagoryMovedFiles).ToDictionary(x => x.Key, x => x.Value); ;
      }
      return result;
    }

    internal void Validate(PlexItem item)
    {
      item.HasWarnings = false;
      foreach (IPlexCatagory catagory in catagories)
      {
        catagory.Validate(item);
      }
    }

    internal bool HasChanged(PlexItem item)
    {
      foreach (IPlexCatagory catagory in catagories)
      {
        if (catagory.HasChanges(item)) return true;
      }
      return false;
    }

    internal bool ValidateChanges(PlexItem item)
    {
      foreach (IPlexCatagory catagory in catagories)
      {
        if (!catagory.ValidateChanges(item)) return false;
      }
      return true;
    }
  }
}
