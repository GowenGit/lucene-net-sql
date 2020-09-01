using System.IO;
using Xunit;

namespace Lucene.Net.Sql.Tests
{
    public class ResourceTests
    {
        [Fact]
        public void Load_WhenCalled_ShouldLoadEmbeddedResource()
        {
            var assembly = typeof(SqlDirectory).Assembly;

            var names = assembly.GetManifestResourceNames();

            Assert.Single(names);
        }

        [Fact]
        public void Load_WhenCalled_ShouldLoadString()
        {
            const string name = "Lucene.Net.Sql.Schema.init_mysql.sql";

            var assembly = typeof(SqlDirectory).Assembly;

            using var stream = assembly.GetManifestResourceStream(name);

            Assert.NotNull(stream);

            using var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();

            Assert.Equal("Hello", result);
        }
    }
}
