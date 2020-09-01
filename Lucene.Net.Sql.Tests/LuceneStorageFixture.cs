using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Xunit;

#pragma warning disable CA1711

namespace Lucene.Net.Sql.Tests
{
    public sealed class LuceneStorageFixture : IDisposable
    {
        public LuceneStorageFixture()
        {
            var options = new SqlDirectoryOptions(SqlDirectoryEngine.MySql, "...", "TestDirectory");

            const LuceneVersion appLuceneVersion = LuceneVersion.LUCENE_48;

            Directory = new SqlDirectory(options);

            Analyzer = new StandardAnalyzer(appLuceneVersion);

            Writer = new IndexWriter(Directory, new IndexWriterConfig(appLuceneVersion, Analyzer));
        }

        public void Dispose()
        {
            Writer.Dispose();
            Analyzer.Dispose();
            Directory.Dispose();
        }

        public SqlDirectory Directory { get; }

        public IndexWriter Writer { get; }

        public Analyzer Analyzer { get; }

        public IndexSearcher CreateSearcher()
        {
            return new IndexSearcher(Writer.GetReader(applyAllDeletes: true));
        }
    }

    [CollectionDefinition("Lucene storage collection")]
    public class LuceneStorageCollection : ICollectionFixture<LuceneStorageFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}