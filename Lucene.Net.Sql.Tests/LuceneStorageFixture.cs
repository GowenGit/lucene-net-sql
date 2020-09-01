using System;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Xunit;

#pragma warning disable CA1711

namespace Lucene.Net.Sql.Tests
{
    public sealed class LuceneStorageFixture : IDisposable
    {
        public LuceneStorageFixture()
        {
            var options = new SqlDirectoryOptions
            {
                DirectoryName = "TestDirectory",
                ConnectionString = "..."
            };

            Directory = new SqlDirectory(options);

            var appLuceneVersion = LuceneVersion.LUCENE_48;

            var analyzer = new StandardAnalyzer(appLuceneVersion);

            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
        }

        public void Dispose()
        {
            Directory.Dispose();
        }

        public SqlDirectory Directory { get; }

        public IndexWriter Writer { get; }
    }

    [CollectionDefinition("Lucene storage collection")]
    public class LuceneStorageCollection : ICollectionFixture<LuceneStorageFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}