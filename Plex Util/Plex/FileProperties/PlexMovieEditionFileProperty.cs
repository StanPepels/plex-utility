using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plex_Util
{

  public class PlexMovieEditionFileProperty : PlexFilePropertyBase<string>
  {
    public override string Name => "Edition";

    public override bool CheckValue()
    {
      return string.IsNullOrEmpty(Value) || ValidationUtils.CheckWindowsFileName(Value);
    }

    public override string DecoratePlexName(string suggestedName)
    {
      return string.IsNullOrEmpty(Value) ? suggestedName : $"{suggestedName} {{edition-{Value}}}";
    }

    public override void ForFile(string file)
    {
      Match match = Regex.Match(file, @"\{edition-(.*?)\}");
      if (match.Success)
      {
        Value = match.Groups[1].Value;
      }
    }
  }
}
