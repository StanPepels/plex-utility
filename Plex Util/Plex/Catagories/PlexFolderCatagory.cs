using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Plex_Util
{
  public class PlexFolderCatagory : IPlexCatagory
  {
    public string Catagory => catagory;
    public ObservableCollection<PlexFolder> Folders => folders;
    public string ParentPath => parentPath;
    private string catagory;
    private ObservableCollection<PlexFolder> folders;
    private string parentPath;
    private IPlexFolderStrategy folderStrategy;

    public PlexFolderCatagory(string name, string parentPath, IPlexFolderStrategy folderStrategy)
    {
      this.parentPath = parentPath;
      this.folders = new ObservableCollection<PlexFolder>();
      this.folderStrategy = folderStrategy;
      catagory = name;
      foreach (PlexFolder folder in folderStrategy.ListFolders(parentPath))
      {
        folders.Add(folder);
      }
    }

    public IPlexCatagory FindCatagoryForFile(PlexFile file)
    {
      foreach (PlexFolder folder in folders)
      {
        IPlexCatagory catagory = folder.FindFileCatagory(file);
        if (catagory != null) return catagory;
      }
      return null;
    }

    public Dictionary<string, string> MoveContentTo(PlexItem item, string tempFilePath)
    {
      Dictionary<string, string> result = new Dictionary<string, string>();
      foreach (PlexFolder folder in folders)
      {
        string newName = folderStrategy.FolderName;
        foreach (IPlexFileProperty property in folder.Properties)
        {
          newName = property.DecoratePlexName(newName);
        }
        Dictionary<string, string> movedItems = folder.MoveFiles(item, Path.Combine(tempFilePath, newName));
        result = result.Concat(movedItems).ToDictionary(x => x.Key, x => x.Value); ;
      }
      return result;
    }

    public void AddFolder()
    {
      folders.Add(folderStrategy.CreateNew(parentPath, folders));
    }

    public bool TryRemoveFolder(PlexFolder folder)
    {
      if (folder.GetFileCount() > 0)
      {
        MessageBox.Show($"Cannot remove folder with files in them.", "Not empty", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }
      folders.Remove(folder);
      return true;
    }

    public int GetFileCount()
    {
      return folders.Sum(f => f.GetFileCount());
    }

    public IPlexCatagory FindCatagoryForFolder(PlexFolder toFind)
    {
      foreach (PlexFolder folder in folders)
      {
        if (folder == toFind) return this;
      }
      return null;
    }

    public void Validate(PlexItem item)
    {
      foreach (PlexFolder folder in folders)
      {
        folder.Validate(item);
      }
    }

    public bool HasChanges(PlexItem item)
    {
      foreach (PlexFolder folder in folders)
      {
        if (folder.HasChanged(item)) return true;
      }
      return false;
    }

    public bool ValidateChanges(PlexItem item)
    {
      foreach (PlexFolder folder in folders)
      {
        if (!folder.ValidateChanges(item)) return false;
      }
      return true;
    }
  }
}
