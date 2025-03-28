using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  /// <summary>
  /// A catagory that holds media files.
  /// </summary>
  public class PlexFileCatagory : IPlexCatagory
  {
    public string Catagory => catagory;
    public ObservableCollection<PlexFile> Files => files;
    public List<PlexFile> deletedFiles;
    public string ParentPath => parentPath;

    private ObservableCollection<PlexFile> files;
    private string catagory;
    private string parentPath;
    private IPlexFileStrategy fileStrategy;
    private bool createFolder;

    public PlexFileCatagory(string name, string parentPath, IPlexFileStrategy fileStrategy, bool createFolder = true)
    {
      this.parentPath = parentPath;
      this.catagory = name;
      this.fileStrategy = fileStrategy;
      this.files = new ObservableCollection<PlexFile>();
      this.createFolder = createFolder;
      this.deletedFiles = new List<PlexFile>();
      string path = createFolder ? Path.Combine(parentPath, name) : parentPath;
      if (Directory.Exists(path))
      {
        foreach (string file in Directory.GetFiles(path))
        {
          if (!file.EndsWith(".mkv")) continue;
          List<IPlexFileProperty> fileProperties = fileStrategy.CreateFileProperties();
          foreach (IPlexFileProperty fileProperty in fileProperties) fileProperty.ForFile(file);
          PlexFile plexfile = new PlexFile(file, fileProperties);
          files.Add(plexfile);
        }
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

    public void AddFile(PlexFile file)
    {
      List<IPlexFileProperty> fileProperties = fileStrategy.CreateFileProperties();
      foreach (IPlexFileProperty fileProperty in fileProperties) fileProperty.ForFile(file.FilePath);
      PlexFile plexfile = new PlexFile(file.FilePath, fileProperties);
      files.Add(plexfile);
    }

    public Dictionary<string, string> MoveContentTo(PlexItem item, string tempFilePath)
    {
      Dictionary<string, string> result = new Dictionary<string, string>();
      string destination = createFolder ? Path.Combine(tempFilePath, catagory) : tempFilePath;
      if (files.Count > 0) Directory.CreateDirectory(destination);
      foreach (PlexFile file in files)
      {
        string extension = Path.GetFileName(file.FilePath).Substring(Path.GetFileNameWithoutExtension(file.FilePath).Length);
        string newName = fileStrategy.GetFileName(item, file);
        foreach (IPlexFileProperty property in file.Properties)
        {
          newName = property.DecoratePlexName(newName);
        }
        newName += extension;
        try
        {
          File.Move(file.FilePath, Path.Combine(destination, newName));
          result.Add(Path.Combine(destination, newName), file.FilePath);
        }
        catch (Exception e)
        {
          continue;
        }
      }
      foreach (PlexFile file in deletedFiles)
      {
        if (File.Exists(file.FilePath))
        {
          File.Delete(file.FilePath);
        }
      }
      deletedFiles.Clear();
      return result;
    }

    public int GetFileCount()
    {
      return files.Count;
    }


    public IPlexCatagory FindCatagoryForFolder(PlexFolder toFind)
    {

      return null;
    }

    public void Validate(PlexItem item)
    {
      string destination = createFolder ? Path.Combine(parentPath, catagory) : parentPath;
      foreach (PlexFile file in files)
      {
        foreach (IPlexFileProperty property in file.Properties)
        {
          if (!property.IsValidValue)
          {
            item.HasWarnings = true;
          }
        }
        string extension = Path.GetFileName(file.FilePath).Substring(Path.GetFileNameWithoutExtension(file.FilePath).Length);
        string newName = fileStrategy.GetFileName(item, file);
        foreach (IPlexFileProperty property in file.Properties)
        {
          newName = property.DecoratePlexName(newName);
        }
        newName += extension;
        if (Path.Combine(destination, newName) != file.FilePath)
        {
          item.HasWarnings = true;
        }
      }
    }

    public bool HasChanges(PlexItem item)
    {
      string destination = createFolder ? Path.Combine(parentPath, catagory) : parentPath;
      foreach (PlexFile file in files)
      {
        string extension = Path.GetFileName(file.FilePath).Substring(Path.GetFileNameWithoutExtension(file.FilePath).Length);
        string newName = fileStrategy.GetFileName(item, file);
        foreach (IPlexFileProperty property in file.Properties)
        {
          newName = property.DecoratePlexName(newName);
        }
        newName += extension;
        try
        {
          if (!File.Exists(Path.Combine(destination, newName))) return true;
        }
        catch (Exception e)
        {
          continue;
        }
      }
      return false;
    }

    internal void RemoveFile(PlexFile file)
    {
      if (Files.Remove(file))
      {
        deletedFiles.Add(file);
      }
    }

    public bool ValidateChanges(PlexItem item)
    {
      HashSet<string> names = new HashSet<string>();
      foreach (PlexFile file in files)
      {
        string extension = Path.GetFileName(file.FilePath).Substring(Path.GetFileNameWithoutExtension(file.FilePath).Length);
        string newName = fileStrategy.GetFileName(item, file);
        foreach (IPlexFileProperty property in file.Properties)
        {
          newName = property.DecoratePlexName(newName);
        }
        newName += extension;

        if (!names.Add(newName))
        {
          return false; // we do not allow duplicate names in the folder.
        }

        foreach (IPlexFileProperty property in file.Properties)
        {
          if (!property.IsValidValue) return false;
        }
      }

      return true;
    }
  }
}
