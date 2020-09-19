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
        public void FuzzySearchMySqlLightNovel()
        {
            FuzzySearch(MySqlWriter);
        }

        [Benchmark]
        public void FuzzySearchFileSystemLightNovel()
        {
            FuzzySearch(FileWriter);
        }

        [Benchmark]
        public void QuerySearchMySqlLightNovel()
        {
            QuerySearch(MySqlWriter);
        }

        [Benchmark]
        public void QuerySearchFileSystemLightNovel()
        {
            QuerySearch(FileWriter);
        }

        private static void IndexData(IndexWriter writer)
        {
            var text = File.ReadAllText("Data/data_01");

            var lines = text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            for (var i = 0; i < lines.Length; i++)
            {
                var doc = new Document
                {
                    new Int32Field(
                        "line",
                        i,
                        Field.Store.YES),
                    new TextField(
                        "text",
                        lines[i],
                        Field.Store.YES)
                };

                writer.AddDocument(doc);
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