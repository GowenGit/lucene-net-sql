using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;

namespace Lucene.Net.Sql.Performance
{
    public class LuceneLiteBenchmark : LuceneBenchmarkBase
    {
        public override void GlobalSetup()
        {
            base.GlobalSetup();

            base.Setup();

            IndexData(MySqlWriter);
            IndexData(FileWriter);

            base.Cleanup();
        }

        [Benchmark]
        public void FuzzySearchMySqlTenNovels()
        {
            FuzzySearch(MySqlWriter);
        }

        [Benchmark]
        public void FuzzySearchFileSystemTenNovels()
        {
            FuzzySearch(FileWriter);
        }

        [Benchmark]
        public void QuerySearchMySqlTenNovels()
        {
            QuerySearch(MySqlWriter);
        }

        [Benchmark]
        public void QuerySearchFileSystemTenNovels()
        {
            QuerySearch(FileWriter);
        }

        private static void IndexData(IndexWriter writer)
        {
            for (var i = 1; i <= 10; i++)
            {
                var file = $"Data/data_{i:00}";

                var lines = File.ReadAllText(file).Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

                for (var j = 0; j < lines.Length; j++)
                {
                    var doc = new Document
                    {
                        new Int32Field(
                            "line",
                            j,
                            Field.Store.YES),
                        new TextField(
                            "text",
                            lines[j],
                            Field.Store.YES)
                    };

                    writer.AddDocument(doc);
                }
            }

            writer.Flush(false, false);
        }

        private static void FuzzySearch(IndexWriter writer)
        {
            var phrase = new FuzzyQuery(new Term("text", "Mary"));

            var searcher = new IndexSearcher(writer.GetReader(true));

            var result = searcher.Search(phrase, 200);

            var _ = result.ScoreDocs;
        }

        private void QuerySearch(IndexWriter writer)
        {
            var parser = new QueryParser(AppLuceneVersion, "text", Analyzer);

            var searcher = new IndexSearcher(writer.GetReader(true));

            var result = searcher.Search(parser.Parse("Mrs. Bennet was quite disconcerted"), 10);

            var _ = result.ScoreDocs;
        }
    }
}