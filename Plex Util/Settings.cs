using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Plex_Util
{
  [XmlRoot("Setting")]
  public class Settings
  {
    [XmlElement("OutputPath")]
    public string OutputPath { get; set; }
    [XmlElement("InputPath")]
    public string InputPath { get; set; }
    [XmlElement("EncodePath")]
    public string EncodePath { get; set; }
    [XmlElement("EncodePreset")]
    public string EncodePreset { get; set; }
    [XmlElement("Preset")]
    public string Preset { get; set; }
    [XmlElement("PlexInputPath")]
    public string PlexInputPath { get; set; }
    [XmlElement("SkipExisting")]
    public bool SkipExisting { get; set; }

    [XmlElement("HandbrakeCliOverride")]
    public string HandbrakeCliOverride { get; set; }
    [XmlElement("HandbrakePathOverride")]
    public string HandbrakePathOverride { get; set; }
    [XmlElement("WinSCPPathOverride")]
    public string WinSCPPathOverride { get; set; }
    [XmlElement("VLCPathOverride")]
    public string VLCPathOverride { get; set; }
    [XmlElement("MakeMKVPathOverride")]
    public string MakeMKVPathOverride { get; set; }


    private static string settingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Media-Util", "settings.xml");
    public static bool TryLoad(out Settings settings)
    {
      settings = null;
      if (File.Exists(settingsPath))
      {
        XmlSerializer serializer = new XmlSerializer(typeof(Settings));
        using (StreamReader reader = new StreamReader(settingsPath))
        {
          settings = (Settings)serializer.Deserialize(reader);
        }

      }
      return settings != null;
    }

    public static void Save(Settings settings)
    {
      try
      {
        XmlSerializer serializer = new XmlSerializer(typeof(Settings));
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
        using (StreamWriter writer = new StreamWriter(settingsPath))
        {
          serializer.Serialize(writer, settings);
        }
      }
      catch (Exception e)
      {
        MessageBox.Show($"Error occured while trying to save settings: {e.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
    }
  }
}
