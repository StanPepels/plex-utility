using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Plex_Util
{
  public class PlexSeasonsFolderStrategy : IPlexFolderStrategy
  {
    public PlexFolder CreateNew(string path, IReadOnlyList<PlexFolder> exisitng)
    {
      int index = 1;
      string defaultName = $"Season {(index).ToString("D2")}";
      bool exists = true;
      while (exists)
      {
        exists = false;
        foreach (PlexFolder folder in exisitng)
        {
          if (folder.Name == defaultName)
          {
            exists = true;
            break;
          }
        }
        if (exists)
        {
          defaultName = $"Season {(++index).ToString("D2")}";
        }
      }
      PlexSeasonNumberFileProperty seasonNumberFileProperty = new PlexSeasonNumberFileProperty();
      return new PlexFolder(Path.Combine(path, defaultName), assets, GetCatagories(Path.Combine(path, defaultName), seasonNumberFileProperty), new IPlexFileProperty[] { seasonNumberFileProperty });
    }

    public List<PlexFolder> ListFolders(string path)
    {
      List<PlexFolder> folders = new List<PlexFolder>();
      foreach (string directory in Directory.GetDirectories(path))
      {
        string dirName = Path.GetFileName(directory);
        Match match = Regex.Match(dirName, @"Season (\d{2})");
        if (match.Success)
        {
          int index = int.Parse(match.Groups[1].Value);
          PlexSeasonNumberFileProperty seasonNumberFileProperty = new PlexSeasonNumberFileProperty();
          folders.Add(new PlexFolder(directory, assets, GetCatagories(directory, seasonNumberFileProperty), new IPlexFileProperty[] { seasonNumberFileProperty }));
        }
      }
      return folders;
    }

    private IPlexCatagory[] GetCatagories(string path, PlexSeasonNumberFileProperty seasonNumberFileProperty) => new IPlexCatagory[] {
        new PlexFileCatagory( "Episodes", path, new PlexSeasonEpisodesFileStrategy(() => seasonNumberFileProperty.Value), false),
        new PlexFileCatagory("Behind The Scenes", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Deleted Scenes", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Featurettes", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Interviews", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Scenes", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Shorts", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Trailers", path, new PlexExtraFileStrategy()),
        new PlexFileCatagory( "Other", path, new PlexExtraFileStrategy())
      };

    private IPlexAsset[] assets => new IPlexAsset[] {
        new PlexPosterAsset()
      };


    public string FolderName => "Season";
  }

}
