using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Plex_Util
{
  /// <summary>
  /// Utiltiy for executing the handbrake cli to scan and encode titles.
  /// </summary>
  public static class Handbrake
  {
    /// <summary>
    /// Scans a file / folder path for titles and returns the title indices which can be passed to select what title to encode later.
    /// </summary>
    public static async Task<(int exitcode, int[] titleIndices)> Scan(string outputPath, CancellationTokenSource cancellationTokenSource)
    {
      List<int> indices = new List<int>();
      ProcessStartInfo handbrakeScanProcess = new ProcessStartInfo();
      handbrakeScanProcess.FileName = App.HandbrakeCliPath;
      handbrakeScanProcess.Arguments = $"-t 0 -i \"{outputPath}\"";
      handbrakeScanProcess.UseShellExecute = false;
      handbrakeScanProcess.CreateNoWindow = true;
      handbrakeScanProcess.RedirectStandardError = true;
      handbrakeScanProcess.RedirectStandardOutput = true;

      int exitCode = await ProcessUtils.RunProcessToCompletionAsync(handbrakeScanProcess, cancellationTokenSource, (s, evt) =>
      {
        App.Log.WriteLine($"[Handbrake] [Info]  : {evt.Data}");
        Match match = Regex.Match(evt.Data, @"\+\stitle\s(\d+)");
        if (match.Success)
        {
          indices.Add(int.Parse(match.Groups[1].Value));
        }

      }, (s, evt) =>
      {
        App.Log.WriteLine($"[Handbrake] [Info]  : {evt.Data}");
        Match match = Regex.Match(evt.Data, @"\+\stitle\s(\d+)");
        if (match.Success)
        {
          indices.Add(int.Parse(match.Groups[1].Value));
        }
      });

      return (exitCode, indices.ToArray());
    }

    /// <summary>
    /// Encodes a title using a preset from a preset file.
    /// </summary>
    /// <param name="encodePresetFilePath">The preset file to read the settings from</param>
    /// <param name="presetName">The name of the preset to use from the preset file.</param>
    /// <param name="titleIndex">The index of the title to encode. <see cref="Handbrake.Scan(string, CancellationTokenSource)"/></param>
    /// <param name="inputPath">The file / folder path to encode</param>
    /// <param name="outputPath">The folder the encoded fiel will be placed in</param>
    /// <param name="cancellationTokenSource"></param>
    /// <param name="progress">Callback that can be used to handle progress updates on the encode.</param>
    /// <returns></returns>
    public static async Task<int> Encode(string encodePresetFilePath, string presetName, int titleIndex, string inputPath, string outputPath, CancellationTokenSource cancellationTokenSource, Action<int> progress = null)
    {
      ProcessStartInfo handbrakeProcess = new ProcessStartInfo();
      handbrakeProcess.FileName = App.HandbrakeCliPath;
      handbrakeProcess.Arguments = $"--min-duration 0 --preset-import-file \"{encodePresetFilePath}\" -Z \"{presetName}\" -t {titleIndex} -i \"{inputPath}\" -o \"{outputPath}\"";
      handbrakeProcess.UseShellExecute = false;
      handbrakeProcess.CreateNoWindow = true;
      handbrakeProcess.RedirectStandardError = true;
      handbrakeProcess.RedirectStandardOutput = true;

      int exitCode = await ProcessUtils.RunProcessToCompletionAsync(handbrakeProcess, cancellationTokenSource, (s, evt) =>
      {
        App.Log.WriteLine($"[Handbrake] [Info]  : {evt.Data}");
        Match match = Regex.Match(evt.Data, @".*?(\d{1,3})\.\d{2} %");
        if (match.Success)
        {
          int current = int.Parse(match.Groups[1].Value);
          progress?.Invoke(current);
        }
      },
      (s, evt) =>
      {
        App.Log.WriteLine($"[Handbrake] [Info]  : {evt.Data}");
      });

      return exitCode;
    }

  }
}
