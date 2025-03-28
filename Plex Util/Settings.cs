using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Plex_Util
{

  [XmlRoot("Setting")]
  public class Settings
  {
    /// <summary>
    /// The output path of the converter tab. this is where all mkv files will bestored from a .iso or blu-ray folder
    /// </summary>
    [XmlElement("OutputPath")]
    public string OutputPath { get; set; }
    /// <summary>
    /// Folder that will be scanned for input for the converter.
    /// </summary>
    [XmlElement("InputPath")]
    public string InputPath { get; set; }
    /// <summary>
    /// The output path of the encode step. This is where the encoded titles will be placed after the convert step has converted the .iso / blu-ray folder to .mkv files
    /// </summary>
    [XmlElement("EncodePath")]
    public string EncodePath { get; set; }
    /// <summary>
    /// Path a json file containing an array of handbrake presets
    /// </summary>
    [XmlElement("EncodePreset")]
    public string EncodePreset { get; set; }
    /// <summary>
    /// The preset to use for the encode step
    /// </summary>
    [XmlElement("Preset")]
    public string Preset { get; set; }
    /// <summary>
    /// Input path for the plex metadata tab. 
    /// </summary>
    [XmlElement("PlexInputPath")]
    public string PlexInputPath { get; set; }
    /// <summary>
    /// If set will skip processing of items that already have been converted when using the converter
    /// </summary>
    [XmlElement("SkipExisting")]
    public bool SkipExisting { get; set; }

    /// <summary>
    /// Override path for the location of the handbrakecli.exe. Can be used in case it is not found in a default location
    /// </summary>
    [XmlElement("HandbrakeCliOverride")]
    public string HandbrakeCliOverride { get; set; }
    /// <summary>
    /// Override path for the location of the handbrake.exe. Can be used in case it is not found in a default location
    /// </summary>
    [XmlElement("HandbrakePathOverride")]
    public string HandbrakePathOverride { get; set; }
    /// <summary>
    /// Override path for the location of the winscp.exe. Can be used in case it is not found in a default location
    /// </summary>
    [XmlElement("WinSCPPathOverride")]
    public string WinSCPPathOverride { get; set; }
    /// <summary>
    /// Override path for the location of the vlc.exe. Can be used in case it is not found in a default location
    /// </summary>
    [XmlElement("VLCPathOverride")]
    public string VLCPathOverride { get; set; }
    /// <summary>
    /// Override path for the location of the makemkvcon.exe. Can be used in case it is not found in a default location
    /// </summary>
    [XmlElement("MakeMKVPathOverride")]
    public string MakeMKVPathOverride { get; set; }

    private static string settingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Media-Util", "settings.xml");

    /// <summary>
    /// Loads the settings from the default location if it exists.
    /// </summary>
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

    /// <summary>
    /// Writes the settings to disk
    /// </summary>
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
