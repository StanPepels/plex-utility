using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Plex_Util
{
  public static class Handbrake
  {
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

    public static async Task<int> Encode(string encodePresetFilePath, string presetName, int titleIndex, string inputPath, string outputPath, MakeMKVItem item, CancellationTokenSource cancellationTokenSource, Action<int> progress = null)
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
          if (current != item.CurrentProgress)
          {
            progress?.Invoke(current);
          }
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
