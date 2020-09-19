using BenchmarkDotNet.Attributes;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Sql.MySql;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = System.IO.Directory;

namespace Lucene.Net.Sql.Performance
{
    public class LuceneBenchmarkBase
    {
        protected const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        protected const string IndexLocation = "D:/git/lucene-net-sql/Lucene.Net.Sql.Performance/bin/Index";

        protected SqlDirectoryOptions Options { get; set; }
        protected Analyzer Analyzer { get; set; }

        protected MySqlLuceneOperator MySqlOperator { get; set; }
        protected LuceneSqlDirectory MySqlDirectory { get; set; }
        protected IndexWriter MySqlWriter { get; set; }

        protected FSDirectory FileDirectory { get; set; }
        protected IndexWriter FileWriter { get; set; }

        [GlobalSetup]
        public virtual void GlobalSetup()
        {
            const string connectionString = "server=localhost;port=3306;database=lucene;uid=root;password=password;Allow User Variables=True;";

            Options = new SqlDirectoryOptions(connectionString, "BenchmarkDirectory");
            Analyzer = new StandardAnalyzer(AppLuceneVersion);
        }

        [IterationSetup]
        public virtual void Setup()
        {
            MySqlOperator = MySqlLuceneOperator.Create(Options);
            MySqlDirectory = new LuceneSqlDirectory(Options, MySqlOperator);
            MySqlWriter = new IndexWriter(MySqlDirectory, new IndexWriterConfig(AppLuceneVersion, Analyzer));

            FileDirectory = FSDirectory.Open(IndexLocation);
            FileWriter = new IndexWriter(FileDirectory, new IndexWriterConfig(AppLuceneVersion, Analyzer));
        }

        [IterationCleanup]
        public virtual void Cleanup()
        {
            MySqlWriter.Dispose();
            MySqlDirectory.Dispose();
            MySqlOperator.Dispose();

            FileWriter.Dispose();
            FileDirectory.Dispose();
        }

        [GlobalCleanup]
        public virtual void GlobalCleanup()
        {
            Analyzer.Dispose();

            Directory.Delete(IndexLocation, true);

            using var mySqlOperator = MySqlLuceneOperator.Create(Options);

            mySqlOperator.PurgeTables();
        }
    }
}