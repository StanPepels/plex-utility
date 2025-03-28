using MarkdownSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Plex_Util
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {

    public event PropertyChangedEventHandler PropertyChanged;

    private const string ReadMePath = "ReadMe.md";


    public bool SkipExisingItems
    {
      get => encodeTab.SkipExisingItems;
      set
      {
        if (encodeTab.SkipExisingItems != value)
        {
          encodeTab.SkipExisingItems = value;
          OnPropertyChanged(nameof(SkipExisingItems));
        }
      }
    }

    public MainWindow()
    {
      InitializeComponent();
      DataContext = this;
      GenerateHelpPage();
      Load();
    }

    protected override void OnActivated(EventArgs e)
    {
      base.OnActivated(e);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override void OnDeactivated(EventArgs e)
    {
      base.OnDeactivated(e);
      Save();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      e.Cancel = encodeTab.StopRunningTask();
    }

    private void Save()
    {
      if(!Settings.TryLoad(out Settings settings))
      {
        settings = new Settings();
      }

      settings.OutputPath = encodeTab.Output;
      settings.InputPath = encodeTab.Input;
      settings.SkipExisting = encodeTab.SkipExisingItems;
      settings.PlexInputPath = plexTab.PlexInput;
      settings.EncodePath = encodeTab.Encode;
      settings.EncodePreset = encodeTab.EncodePresetPath;
      settings.Preset = encodeTab.Preset;
      Settings.Save(settings);
    }

    private void Load()
    {
      try
      {
        if (Settings.TryLoad(out Settings settings))
        {
          encodeTab.Output = settings.OutputPath;
          encodeTab.Input = settings.InputPath;
          encodeTab.SkipExisingItems = settings.SkipExisting;
          plexTab.PlexInput = settings.PlexInputPath;
          encodeTab.Encode = settings.EncodePath;
          encodeTab.EncodePresetPath = settings.EncodePreset;
          encodeTab.Preset = settings.Preset;
        }
      }
      catch (Exception e)
      {
        ShowExceptionMessage(e);
      }
    }

   private string GetReadMePagePath() => Path.Combine(Path.GetFileNameWithoutExtension(ReadMePath) + ".html");


    private string GenerateHelpPage()
    {
      string markdownText = File.ReadAllText(ReadMePath);
      string outputPath = GetReadMePagePath();
      Markdown markdown = new Markdown();
      string htmlContent = markdown.Transform(markdownText);
      File.WriteAllText(outputPath, $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <link rel=""stylesheet"" href=""ReadMe.css"">
    <title>Generated HTML</title>
</head>
<body>
    {htmlContent}
</body>
</html>");
      return outputPath;
    }

    private void ReadMe_Click(object sender, RoutedEventArgs e)
    {
      string htmlPage = GetReadMePagePath();
      ProcessStartInfo openCmd = new ProcessStartInfo();
      openCmd.FileName = "cmd";
      openCmd.Arguments = $"/c \"{htmlPage}\"";
      openCmd.UseShellExecute = false;
      openCmd.CreateNoWindow = true;
      Process.Start(openCmd);
    }

    private void ShowExceptionMessage(Exception e)
    {
      MessageBox.Show($"Error occured while trying to save settings: {e.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void HandlePreferencesMenuItemClickedEvent(object sender, RoutedEventArgs e)
    {
      OptionsWindow optionsWindow = new OptionsWindow();
      optionsWindow.Owner = Application.Current.MainWindow;
      optionsWindow.ShowDialog();
      App.UpdateDependencies();
    }
  }
}
