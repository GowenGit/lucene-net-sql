using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Sql.MySql;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = System.IO.Directory;

namespace Lucene.Net.Sql.Performance
{
    public class LuceneBenchmark
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private const string IndexLocation = "D:/git/lucene-net-sql/Lucene.Net.Sql.Performance/bin/Index";

        private SqlDirectoryOptions _options;
        private Analyzer _analyzer;

        private MySqlLuceneOperator _mySqlOperator;
        private LuceneSqlDirectory _mySqlDirectory;
        private IndexWriter _mySqlWriter;

        private FSDirectory _fileDirectory;
        private IndexWriter _fileWriter;

        [GlobalSetup]
        public void GlobalSetup()
        {
            const string connectionString = "server=localhost;port=3306;database=lucene;uid=root;password=password;Allow User Variables=True;";

            _options = new SqlDirectoryOptions(connectionString, "BenchmarkDirectory");
            _analyzer = new StandardAnalyzer(AppLuceneVersion);
        }

        [IterationSetup]
        public void Setup()
        {
            _mySqlOperator = MySqlLuceneOperator.Create(_options);
            _mySqlDirectory = new LuceneSqlDirectory(_options, _mySqlOperator);
            _mySqlWriter = new IndexWriter(_mySqlDirectory, new IndexWriterConfig(AppLuceneVersion, _analyzer));

            _fileDirectory = FSDirectory.Open(IndexLocation);
            _fileWriter = new IndexWriter(_fileDirectory, new IndexWriterConfig(AppLuceneVersion, _analyzer));
        }

        [Benchmark]
        public void IndexAndFetchMySql()
        {
            IndexData(_mySqlWriter);

            FetchData(_mySqlWriter);
        }

        [Benchmark]
        public void IndexAndFetchFileSystem()
        {
            IndexData(_fileWriter);

            FetchData(_fileWriter);
        }

        private static void IndexData(IndexWriter writer)
        {
            var text = File.ReadAllText("Data/data_01.txt");

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


        private static void FetchData(IndexWriter writer)
        {
            var phrase = new FuzzyQuery(new Term("text", "Mary"));

            var searcher = new IndexSearcher(writer.GetReader(true));

            var result = searcher.Search(phrase, 200);

            var _ = result.ScoreDocs;
        }

        [IterationCleanup]
        public void Cleanup()
        {
            _mySqlWriter.Dispose();
            _mySqlDirectory.Dispose();
            _mySqlOperator.Dispose();

            _fileWriter.Dispose();
            _fileDirectory.Dispose();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _analyzer.Dispose();

            Directory.Delete(IndexLocation, true);

            using var mySqlOperator = MySqlLuceneOperator.Create(_options);

            mySqlOperator.PurgeTables();
        }
    }
}