using BatchProcessing.Core;
using System.Diagnostics;

using var processor = new BatchingProcessor();

var tasks = new Task[4];

for (int i = 1; i <= 4; i++)
{
    int id = i;
    tasks[i - 1] = Task.Run(async () =>
    {
        var sw = Stopwatch.StartNew();
        var result = await processor.ProcessOne(new DataObject { Id = id });
        sw.Stop();
        Console.WriteLine($"Worker {id} finished in {sw.ElapsedMilliseconds}ms with result {result.InputId}");
    });
}

await Task.WhenAll(tasks);
Console.WriteLine("Done");
