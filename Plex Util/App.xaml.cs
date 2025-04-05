using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Plex_Util
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public const string LogPath = "log.txt";
    public static StreamWriter Log;
    private static string MakeMkv;
    private static string Vlc;
    private static string WinScp;
    private static string Handbrake;
    private static string HandbrakeCli;
    private static string installKeyBase = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
    private static string keyBase = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

    public static bool MakeMKVSupported => !string.IsNullOrEmpty(MakeMkv) && File.Exists(MakeMkv);
    public static bool HandbrakeSupported => !string.IsNullOrEmpty(Handbrake) && File.Exists(Handbrake);
    public static bool HandbrakeCliSuppported => !string.IsNullOrEmpty(HandbrakeCli) && File.Exists(HandbrakeCli);
    public static bool WinScpSupported => !string.IsNullOrEmpty(WinScp) && File.Exists(WinScp);
    public static bool VlcSupported => !string.IsNullOrEmpty(Vlc) && File.Exists(Vlc);

    public static string MakeMkvPath => MakeMkv;
    public static string VlcPath => Vlc;
    public static string WinScpPath => WinScp;
    public static string HandBrakePath => Handbrake;
    public static string HandbrakeCliPath => HandbrakeCli;

    public static event Action OnDependenciesUpdated;
    static App()
    {
      UpdateDependencies();
      if(!Power.ApplyShutdownPrivilidge())
      {
        MessageBox.Show("Failed to grant shutdown privileges. See log for details.", "Startup error", MessageBoxButton.OK, MessageBoxImage.Error);
        App.Current.Shutdown();
      }
    }

    public static void UpdateDependencies()
    {
      MakeMkv = GetPathForExe("makemkvcon.exe");
      Vlc = GetPathForExe("vlc.exe");
      WinScp = Path.Combine(GetInstallPath("winscp3_is1"), "WinScp.exe");
      Handbrake = GetPathForExe("HandBrake.exe");
      HandbrakeCli = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "HandBrakeCLI.exe");
      if (Settings.TryLoad(out Settings settings))
      {
        if (!string.IsNullOrEmpty(settings.MakeMKVPathOverride)) MakeMkv = settings.MakeMKVPathOverride;
        if (!string.IsNullOrEmpty(settings.VLCPathOverride)) Vlc = settings.VLCPathOverride;
        if (!string.IsNullOrEmpty(settings.WinSCPPathOverride)) WinScp = settings.WinSCPPathOverride;
        if (!string.IsNullOrEmpty(settings.HandbrakeCliOverride)) HandbrakeCli = settings.HandbrakeCliOverride;
        if (!string.IsNullOrEmpty(settings.HandbrakePathOverride)) Handbrake = settings.HandbrakePathOverride;
      }

      if (!MakeMKVSupported || !HandbrakeSupported || !HandbrakeCliSuppported || !WinScpSupported || !VlcSupported)
      {
        MessageBox.Show("One or more dependencies were not found. Some functionality will be disabled. Paths can be changed via Options -> Preferences.", "Missing dependencies", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
      OnDependenciesUpdated?.Invoke();
    }

    public static void OpenLog()
    {
      ProcessStartInfo openCmd = new ProcessStartInfo();
      openCmd.FileName = "cmd";
      openCmd.Arguments = $"/c notepad \"{App.LogPath}\"";
      openCmd.UseShellExecute = false;
      openCmd.CreateNoWindow = true;
      Process.Start(openCmd);
    }

    private static string GetPathForExe(string fileName)
    {
      RegistryKey localMachine = Registry.LocalMachine;
      RegistryKey currentUser = Registry.CurrentUser;

      RegistryKey fileKey = localMachine.OpenSubKey(string.Format(@"{0}\{1}", keyBase, fileName));
      object result = null;
      if (fileKey == null)
      {
        fileKey = currentUser.OpenSubKey(string.Format(@"{0}\{1}", keyBase, fileName));
      }

      if (fileKey != null)
      {
        result = fileKey.GetValue(string.Empty);
        fileKey.Close();
      }

      return (string)result;
    }

    private static string GetInstallPath(string programName)
    {
      RegistryKey localMachine = Registry.LocalMachine;
      RegistryKey currentUser = Registry.CurrentUser;

      RegistryKey fileKey = localMachine.OpenSubKey(string.Format(@"{0}\{1}", installKeyBase, programName));
      object result = null;
      if (fileKey == null)
      {
        fileKey = currentUser.OpenSubKey(string.Format(@"{0}\{1}", installKeyBase, programName));
      }

      if (fileKey != null)
      {
        result = fileKey.GetValue("InstallLocation");
        fileKey.Close();
      }

      return (string)result;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      CreateLog();
      AppDomain.CurrentDomain.UnhandledException += HandleUhandledException;
    }

    private void HandleUhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      MessageBox.Show($"Encountered an unhandled exception: {e.ExceptionObject.GetType().FullName}.{(e.IsTerminating ? "Application will terminate.": string.Empty)}", "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    protected override void OnExit(ExitEventArgs e)
    {
      Log.Dispose();
    }

    private void CreateLog()
    {
      Log = new StreamWriter(File.Open(LogPath, FileMode.Create, FileAccess.Write, FileShare.Read));
      Log.AutoFlush = true;
    }
  }
}
