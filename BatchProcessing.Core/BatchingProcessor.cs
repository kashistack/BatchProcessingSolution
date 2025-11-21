using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace BatchProcessing.Core;

public class BatchingProcessor : IDisposable
{
    private readonly ConcurrentQueue<(DataObject data, TaskCompletionSource<Result> tcs)> _queue
        = new();

    private readonly SemaphoreSlim _signal = new(0);
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;

    public BatchingProcessor()
    {
        _loop = Task.Run(ProcessLoopAsync);
    }

    public Task<Result> ProcessOne(DataObject obj)
    {
        var tcs = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        _queue.Enqueue((obj, tcs));
        _signal.Release();
        return tcs.Task;
    }

    private async Task ProcessLoopAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            await _signal.WaitAsync(_cts.Token);

            List<(DataObject data, TaskCompletionSource<Result> tcs)> batch = new();

            if (_queue.TryDequeue(out var first)) batch.Add(first);

            var sw = Stopwatch.StartNew();
            while (batch.Count < 4 && sw.ElapsedMilliseconds < 100)
            {
                if (_queue.TryDequeue(out var next)) batch.Add(next);
                else await Task.Delay(10);
            }

            var inputArray = batch.Select(x => x.data).ToArray();

            var results = DataProcessor.Instance.ProcessData(inputArray);

            for (int i = 0; i < batch.Count; i++)
                batch[i].tcs.SetResult(results[i]);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}
