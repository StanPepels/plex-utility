using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Plex_Util
{
  public class ItemProgressUpdater
  {
    private Dispatcher dispatcher;
    private Thread processingTask;
    private CancellationTokenSource cancellationTokenSource;
    private const int updateInterval = 10;
    private ConcurrentDictionary<int, MakeMKVItem> itemsToUpdate;
    private ConcurrentDictionary<MakeMKVItem, int> itemIndices;
    private bool messageLock;
    private int requestCount;
    private Stopwatch stopwatch;

    public ItemProgressUpdater(Dispatcher dispatcher)
    {
      this.dispatcher = dispatcher;
      processingTask = null;
      itemsToUpdate = new ConcurrentDictionary<int, MakeMKVItem>();
      itemIndices = new ConcurrentDictionary<MakeMKVItem, int>();
      stopwatch = new Stopwatch();
    }

    public void Start()
    {
      cancellationTokenSource = new CancellationTokenSource();
  
      processingTask = new Thread(() =>
      {
        stopwatch.Start();
        while (!cancellationTokenSource.IsCancellationRequested)
        {
          if (stopwatch.ElapsedMilliseconds >= updateInterval)
          {
            stopwatch.Reset();
            messageLock = true; // enure no more requests can come in
           // await all request tasks before continueing
            while (requestCount > 0)
            {
              Thread.Sleep(1);
            }
            dispatcher.Invoke(() =>
            {
              try
              {
                while (itemsToUpdate.Count > 0 && itemsToUpdate.TryRemove(itemsToUpdate.Count - 1, out MakeMKVItem item))
                {
                  item.NotifyProgressChanged();
                }
              }
              finally
              {
                itemsToUpdate.Clear();
                itemIndices.Clear();
                messageLock = false;
                stopwatch.Start();
              }
            });
            Thread.Sleep(10);
          }
        }
      });
      processingTask.Start();
    }

    public async Task RequestUpdate(MakeMKVItem item)
    {
      if (messageLock)
      {
        await Task.Yield();
      }
      if (itemIndices.ContainsKey(item)) return;
      Interlocked.Increment(ref requestCount);
      itemsToUpdate.TryAdd(itemsToUpdate.Count, item);
      itemIndices.TryAdd(item, itemIndices.Count);
      Interlocked.Decrement(ref requestCount);
    }
    public void Stop()
    {
      cancellationTokenSource?.Cancel();
      while(processingTask.IsAlive)
      {
        Thread.Sleep(1);
      }
    }
  }
}
