using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Lucene.Net.Sql.MySql.Example
{
    // dotnet pack -c Release -o out --version-suffix 0.0.3-alpha Lucene.Net.Sql.MySql/Lucene.Net.Sql.MySql.csproj
    public class Program
    {
        public static void Main(string[] args)
        {
            const string connectionString = "server=localhost;port=3306;database=lucene;uid=root;password=password;Allow User Variables=True;";

            var options = new SqlDirectoryOptions(connectionString, "ExampleDirectory");

            using var mySqlOperator = MySqlLuceneOperator.Create(options);

            using var directory = new LuceneSqlDirectory(options, mySqlOperator);

            using var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            using var writer = new IndexWriter(directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer));

            var source = new
            {
                Name = "Kermit the Frog",
                FavoritePhrase = "The quick brown fox jumps over the lazy dog"
            };

            var doc = new Document
            {
                new StringField(
                    "name",
                    source.Name,
                    Field.Store.YES),
                new TextField(
                    "favoritePhrase",
                    source.FavoritePhrase,
                    Field.Store.YES)
            };

            writer.AddDocument(doc);

            writer.Flush(false, false);

            var phrase = new MultiPhraseQuery
            {
                new Term("favoritePhrase", "brown"),
                new Term("favoritePhrase", "fox")
            };

            var searcher = new IndexSearcher(writer.GetReader(true));

            var result = searcher.Search(phrase, 20);
        }
    }
}
