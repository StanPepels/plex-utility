using System.IO;

namespace Plex_Util
{
  public class PlexTitleProperty : PlexFilePropertyBase<string>
  {
    public override string Name => "Title";

    public override string DecoratePlexName(string suggestedName)
    {
      return Value;
    }

    public override bool CheckValue()
    {
      return string.IsNullOrEmpty(Value) || ValidationUtils.CheckWindowsFileName(Value);
    }

    public override void ForFile(string file)
    {
      Value = Path.GetFileNameWithoutExtension(file);
    }
  }
}
