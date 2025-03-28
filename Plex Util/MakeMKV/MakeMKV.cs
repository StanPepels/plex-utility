using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Plex_Util
{
  public static class MakeMKV
  {
    public static async Task<int> ExtractAllTitles(MakeMKVItem item, string outputPath, CancellationTokenSource cancellationTokenSource, Dispatcher dispatcher)
    {
      ItemProgressUpdater progressUpdater = new ItemProgressUpdater(dispatcher);
      progressUpdater.Start();
      ProcessStartInfo makeMkvProcess = new ProcessStartInfo();
      makeMkvProcess.FileName = App.MakeMkvPath;
      makeMkvProcess.Arguments = $"mkv \"{(Path.GetExtension(item.FilePath) == ".iso" ? "iso" : "file")}:{item.FilePath}\" all \"{outputPath}\" --robot --messages=-stdout --progress=-stderr";
      makeMkvProcess.UseShellExecute = false;
      makeMkvProcess.CreateNoWindow = true;
      makeMkvProcess.RedirectStandardError = true;
      makeMkvProcess.RedirectStandardOutput = true;

      int exitCode = await ProcessUtils.RunProcessToCompletionAsync(makeMkvProcess, cancellationTokenSource, (sender, args) =>
        {
          ProcessMakeMKVStdout(args.Data, item);
        },
        (sender, args) =>
        {
          ProcessMakeMKVStderr(args.Data, item, progressUpdater);
        }
      );
      progressUpdater.Stop();
      return exitCode;
    }

    public static async Task<int> ExtractTitle(MakeMKVItem item, string outputPath, TitleInfo title, CancellationTokenSource cancellationTokenSource, Dispatcher dispatcher)
    {
      ItemProgressUpdater progressUpdater = new ItemProgressUpdater(dispatcher);
      progressUpdater.Start();
      ProcessStartInfo makeMkvProcess = new ProcessStartInfo();
      makeMkvProcess.FileName = App.MakeMkvPath;
      makeMkvProcess.Arguments = $"mkv \"{(Path.GetExtension(item.FilePath) == ".iso" ? "iso" : "file")}:{item.FilePath}\" \"{item.Titles.IndexOf(title)}\" \"{outputPath}\" --robot --messages=-stdout --progress=-stderr";
      makeMkvProcess.UseShellExecute = false;
      makeMkvProcess.CreateNoWindow = true;
      makeMkvProcess.RedirectStandardError = true;
      makeMkvProcess.RedirectStandardOutput = true;

      int exitCode = await ProcessUtils.RunProcessToCompletionAsync(makeMkvProcess, cancellationTokenSource, (sender, args) =>
      {
        ProcessMakeMKVStdout(args.Data, item);
      },
        (sender, args) =>
        {
          ProcessMakeMKVStderr(args.Data, item, progressUpdater);
        }
      );
      progressUpdater.Stop();
      return exitCode;
    }

    public static async Task<int> ScanTitles(MakeMKVItem item, CancellationTokenSource cancellationTokenSource, Dispatcher dispatcher)
    {
      ItemProgressUpdater progressUpdater = new ItemProgressUpdater(dispatcher);
      progressUpdater.Start();
      ProcessStartInfo makeMkvProcess = new ProcessStartInfo();
      makeMkvProcess.FileName = App.MakeMkvPath;
      makeMkvProcess.Arguments = $"info \"{(Path.GetExtension(item.FilePath) == ".iso" ? "iso" : "file")}:{item.FilePath}\" --robot --messages=-stdout --progress=-stderr";
      makeMkvProcess.UseShellExecute = false;
      makeMkvProcess.CreateNoWindow = true;
      makeMkvProcess.RedirectStandardError = true;
      makeMkvProcess.RedirectStandardOutput = true;
      int exitCode = await ProcessUtils.RunProcessToCompletionAsync(makeMkvProcess, cancellationTokenSource, (sender, args) =>
      {
        ProcessMakeMKVStdout(args.Data, item);
      },
       (sender, args) =>
       {
         ProcessMakeMKVStderr(args.Data, item, progressUpdater);
       }
     );
      item.SelectAllTitles();
      progressUpdater.Stop();
      return exitCode;
    }

    private static void ProcessMakeMKVStderr(string message, MakeMKVItem item, ItemProgressUpdater progressUpdater)
    {
      int seperator = message.IndexOf(':');
      if (seperator == -1) return;
      string messageType = message.Substring(0, seperator);
      switch (messageType)
      {
        case "PRGC":
          {
            string messageRaw = message.Remove(0, messageType.Length + 1);
            ProcessCurrentProgressMessage(messageRaw, item);
            break;
          }
        case "PRGT":
          {
            string messageRaw = message.Remove(0, messageType.Length + 1);
            ProcessProgressTitleMessage(messageRaw, item, string.Empty);
            break;
          }
        case "PRGV":
          {
            string messageRaw = message.Remove(0, messageType.Length + 1);
            ProcessProgressBarMessage(messageRaw, item, progressUpdater);
            break;
          }
      }
    }

    private static void ProcessMakeMKVStdout(string message, MakeMKVItem item)
    {
      App.Log.WriteLine($"[MakeMkv] [Info]  : {message}");
      int seperator = message.IndexOf(':');
      if (seperator == -1) return;
      string messageType = message.Substring(0, seperator);
      switch (messageType)
      {
        case "MSG":
          {
            string messageRaw = message.Remove(0, messageType.Length + 1);
            ProcessMessage(messageRaw, item);
            break;
          }
        case "TINFO":
          {
            string messageRaw = message.Remove(0, messageType.Length + 1);
            ProcessTitleInfo(messageRaw, item);
            break;
          }
        case "TCOUNT":
          {
            string messageRaw = message.Remove(0, messageType.Length + 1);
            TitleInfo[] titles = new TitleInfo[int.Parse(messageRaw)];
            for (int i = 0; i < titles.Length; i++)
            {
              titles[i] = new TitleInfo();
            }
            item.Titles.Clear();
            foreach (TitleInfo title in titles) item.Titles.Add(title);
            break;
          }
      }
    }

    private static void ProcessMessage(string messageRaw, MakeMKVItem item)
    {
      int code = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      int flags = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      int paramCount = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      string text = messageRaw.Substring(0, messageRaw.IndexOf(",\""));
      messageRaw = messageRaw.Remove(0, text.Length + 1);
      string format = paramCount > 0 ? messageRaw.Substring(0, messageRaw.IndexOf(",\"")) : messageRaw;

      string lastValue = format;
      List<string> parameters = new List<string>();
      for (int i = 0; i < paramCount; ++i)
      {
        messageRaw = messageRaw.Remove(0, lastValue.Length + 1);
        parameters.Add(i < paramCount - 1 ? messageRaw.Substring(0, messageRaw.IndexOf(",\"")) : messageRaw);
        lastValue = parameters.Last();
      }
    }



    private static void ProcessTitleInfo(string messageRaw, MakeMKVItem item)
    {
      int titleIndex = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      int id = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      int flags = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      string value = messageRaw.Trim('"');
      switch (id)
      {
        case 27: // name
          item.Titles[titleIndex].Name = value;
          break;
        case 9: // length
          item.Titles[titleIndex].Length = value;
          break;
        case 11: // size in bytes
          item.Titles[titleIndex].Size = long.Parse(value);
          break;
      }
    }

    private static void ProcessCurrentProgressMessage(string messageRaw, MakeMKVItem item)
    {
      int code = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      int id = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      string name = messageRaw.Trim('"');
    }

    private static void ProcessProgressTitleMessage(string messageRaw, MakeMKVItem item, string progressPrefix)
    {
      int code = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      int id = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      string name = messageRaw.Trim('"');
      item.StatusText = $"{progressPrefix} ({name})";
    }

    private static void ProcessProgressBarMessage(string messageRaw, MakeMKVItem item, ItemProgressUpdater progressUpdater)
    {
      int current = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      int total = int.Parse(messageRaw.Substring(0, messageRaw.IndexOf(',')));
      messageRaw = messageRaw.Remove(0, messageRaw.IndexOf(',') + 1);
      int max = int.Parse(messageRaw);
      item.ProgressMaxValue = max;
      item.CurrentProgress = current;
      item.TotalProgress = total;
      progressUpdater.RequestUpdate(item);
    }

  }
}
