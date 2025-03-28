using System.IO;
using System.Text.RegularExpressions;

namespace Plex_Util
{
  public class PlexEpisodeNumberFileProperty : PlexFilePropertyBase<int>
  {
    public override string Name => "Index";

    public override string DecoratePlexName(string suggestedName)
    {
      return $"{suggestedName}e{Value.ToString("D2")}";
    }

    public override void ForFile(string file)
    {
      Match match = Regex.Match(Path.GetFileNameWithoutExtension(file), @"s\d{2}e(\d{2})");
      if (match.Success)
      {
        Value = int.Parse(match.Groups[1].Value);
      }
    }

    public override bool CheckValue()
    {
      return Value >= 1;
    }
  }
}
