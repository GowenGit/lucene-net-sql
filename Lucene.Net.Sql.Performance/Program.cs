using System;
using BenchmarkDotNet.Running;

namespace Lucene.Net.Sql.Performance
{
    public class Program
    {
        public static void Main(string[] args)  
        {
            BenchmarkRunner.Run<LuceneLiteBenchmark>();
            // BenchmarkRunner.Run<LuceneHeavyBenchmark>();
        }
    }
}
