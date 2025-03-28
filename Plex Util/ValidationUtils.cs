using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plex_Util
{
  public class ValidationUtils
  {
    public static bool CheckWindowsFileName(string fileName)
    {
      // List of invalid characters for a Windows file name
      char[] invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|', '\0' };

      // Check if the file name contains any of the invalid characters
      return fileName.IndexOfAny(invalidChars) == -1;
    }
  }
}
