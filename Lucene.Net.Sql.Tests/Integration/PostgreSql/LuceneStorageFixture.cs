using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Sql.Postgre;
using Lucene.Net.Util;
using Xunit;

#pragma warning disable CA1711

namespace Lucene.Net.Sql.Tests.Integration.PostgreSql
{
    public sealed class LuceneStorageFixture : IDisposable
    {
        public LuceneStorageFixture()
        {
            const string connectionString = "server=localhost;port=3306;database=lucene;uid=root;password=password;Allow User Variables=True;";

            Options = new SqlDirectoryOptions(connectionString, "TestDirectory");

            const LuceneVersion appLuceneVersion = LuceneVersion.LUCENE_48;

            PostgreSqlOperator = PostgreSqlLuceneOperator.Create(Options);

            Directory = new LuceneSqlDirectory(Options, PostgreSqlOperator);

            Analyzer = new StandardAnalyzer(appLuceneVersion);

            Writer = new IndexWriter(Directory, new IndexWriterConfig(appLuceneVersion, Analyzer));
        }

        public void Dispose()
        {
            Writer.Dispose();
            Analyzer.Dispose();

            Directory.Dispose();
            PostgreSqlOperator.PurgeTables();
            PostgreSqlOperator.Dispose();
        }

        public PostgreSqlLuceneOperator PostgreSqlOperator { get; }

        public SqlDirectoryOptions Options { get; }

        public LuceneSqlDirectory Directory { get; }

        public IndexWriter Writer { get; }

        public Analyzer Analyzer { get; }

        public IndexSearcher CreateSearcher()
        {
            return new IndexSearcher(Writer.GetReader(true));
        }
    }

    [CollectionDefinition("PostgreSql Lucene storage collection")]
    public class LuceneStorageCollection : ICollectionFixture<LuceneStorageFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}