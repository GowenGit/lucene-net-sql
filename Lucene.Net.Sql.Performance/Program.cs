using System;
using BenchmarkDotNet.Running;

namespace Lucene.Net.Sql.Performance
{
    public class Program
    {
        public static void Main(string[] args)  
        {
            var _ = BenchmarkRunner.Run<LuceneBenchmark>();

            Console.ReadLine();
        }
    }
}
