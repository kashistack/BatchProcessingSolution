using BatchProcessing.Core;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BatchProcessing.Tests
{
    public class BatchingProcessorTests
    {
        [Fact]
        public async Task Single_Request_Completes_Under_2_Seconds()
        {
            using var p = new BatchingProcessor();

            var sw = Stopwatch.StartNew();
            var result = await p.ProcessOne(new DataObject { Id = 1 });
            sw.Stop();

            Assert.Equal(1, result.InputId);
            Assert.True(sw.ElapsedMilliseconds < 2000);
        }

        [Fact]
        public async Task Four_Requests_Batched_Under_2_Seconds()
        {
            using var p = new BatchingProcessor();

            var tasks = Enumerable.Range(1, 4)
                .Select(i => p.ProcessOne(new DataObject { Id = i }))
                .ToArray();

            var sw = Stopwatch.StartNew();
            var results = await Task.WhenAll(tasks);
            sw.Stop();

            Assert.Equal(4, results.Length);
            Assert.All(results, r => Assert.True(r.InputId >= 1 && r.InputId <= 4));
            Assert.True(sw.ElapsedMilliseconds < 2000);
        }

        [Fact]
        public async Task Parallel_Workers_Under_2_Seconds()
        {
            using var p = new BatchingProcessor();

            var tasks = Enumerable.Range(1, 4)
                .Select(i => Task.Run(() => p.ProcessOne(new DataObject { Id = i })))
                .ToArray();

            var sw = Stopwatch.StartNew();
            var results = await Task.WhenAll(tasks);
            sw.Stop();

            Assert.Equal(4, results.Length);
            Assert.True(sw.ElapsedMilliseconds < 2000);
        }
    }
}
