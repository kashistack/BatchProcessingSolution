using System;
using System.Linq;
using System.Threading;

namespace BatchProcessing.Core
{
    public sealed class DataProcessor
    {
        private static readonly Lazy<DataProcessor> _instance =
            new(() => new DataProcessor());

        public static DataProcessor Instance => _instance.Value;

        private DataProcessor() { }

        public Result[] ProcessData(DataObject[] data)
        {
            Thread.Sleep(1000); // GPU always takes 1 second
            return data.Select(d => new Result { InputId = d.Id }).ToArray();
        }
    }
}
