using System.IO;
using System.Text.RegularExpressions;

namespace Plex_Util
{
  public class PlexEpisodeTitleFileProperty : PlexFilePropertyBase<string>
  {
    public override string Name => "Title";

    public override string DecoratePlexName(string suggestedName)
    {
      return string.IsNullOrEmpty(Value) ? suggestedName : $"{suggestedName} - {Value}";
    }

    public override void ForFile(string file)
    {
      Match match = Regex.Match(Path.GetFileNameWithoutExtension(file), @"s\d{2}e\d{2} - (.+)$");
      if (match.Success)
      {
        Value = match.Groups[1].Value;
      }
    }

    public override bool CheckValue()
    {
      return string.IsNullOrEmpty(Value) || ValidationUtils.CheckWindowsFileName(Value);
    }
  }
}
