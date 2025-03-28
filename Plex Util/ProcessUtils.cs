using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plex_Util
{
  public delegate void ProcessOutput(object sender, DataReceivedEventArgs args);
  public static class ProcessUtils
  {
    public async static Task<int> RunProcessToCompletionAsync(ProcessStartInfo processStartInfo, CancellationTokenSource cancellationTokenSource, ProcessOutput stdout = null, ProcessOutput stderr = null)
    {
      Process process = new Process();
      process.StartInfo = processStartInfo;
      process.EnableRaisingEvents = true;
      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      process.OutputDataReceived += (e, args) =>
      {
        if (args.Data != null)
        {
          stdout?.Invoke(e, args);
        }
      };

      process.ErrorDataReceived += (e, args) =>
      {
        if (args.Data != null)
        {
          stderr?.Invoke(e, args);
        }
      };

      await Task.Yield();
      while (!process.HasExited)
      {
        if (cancellationTokenSource.IsCancellationRequested)
        {
          process.Kill();
          process.WaitForExit();
        }
        Thread.Sleep(1);
      }
      return process.ExitCode;
    }

  }
}
