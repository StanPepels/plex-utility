using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class PlexUncatagorizedFileCatagory : IPlexCatagory
  {
    public string Catagory => "Uncatagorized";
    public ObservableCollection<PlexFile> Files => files;
    public string ParentPath => parentPath;

    private ObservableCollection<PlexFile> files;
    private string parentPath;
    public List<PlexFile> deletedFiles;

    public PlexUncatagorizedFileCatagory(string parentPath, IPlexCatagory[] otherCatatgories)
    {
      this.parentPath = parentPath;
      this.deletedFiles = new List<PlexFile>();
      this.files = new ObservableCollection<PlexFile>();
      if (Directory.Exists(parentPath))
      {
        foreach (string file in Directory.GetFiles(parentPath, "*", SearchOption.AllDirectories))
        {
          if (!file.EndsWith(".mkv")) continue;
          PlexFile plexfile = new PlexFile(file, new List<IPlexFileProperty>());
          IPlexCatagory fileCatagory = null;
          foreach (IPlexCatagory catagory in otherCatatgories)
          {
            if ((fileCatagory = catagory.FindCatagoryForFile(plexfile)) != null) break;
          }
          if (fileCatagory == null)
          {
            files.Add(plexfile);
          }
        }
      }
    }

    internal void RemoveFile(PlexFile file)
    {
      if (Files.Remove(file))
      {
        deletedFiles.Add(file);
      }
    }

    public IPlexCatagory FindCatagoryForFile(PlexFile file)
    {
      foreach (PlexFile other in files)
      {
        if (file.FilePath == other.FilePath) return this;
      }
      return null;
    }

    public Dictionary<string, string> MoveContentTo(PlexItem item, string tempFilePath)
    {
      Dictionary<string, string> result = new Dictionary<string, string>();
      foreach (PlexFile file in files)
      {
        File.Delete(file.FilePath);
      }

      foreach (PlexFile file in deletedFiles)
      {
        File.Delete(file.FilePath);
      }
      return result;
    }

    public int GetFileCount()
    {
      return files.Count;
    }

    public IPlexCatagory FindCatagoryForFolder(PlexFolder folder)
    {
      return null;
    }

    public void Validate(PlexItem item)
    {
      if (files.Count > 0)
      {
        item.HasWarnings = true;
      }
    }

    public bool HasChanges(PlexItem item)
    {
      return files.Count > 0; // uncatogorized will be deleted so if it contains files return true.
    }

    public bool ValidateChanges(PlexItem item)
    {
      return files.Count == 0; // if uncatagorized contains files the changes are invalid all files should be catagorized before we can make changes.
    }
  }

}
