using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Xunit;

namespace Lucene.Net.Sql.Tests
{
    [Collection("Lucene storage collection")]
    public class LuceneTests
    {
        private readonly LuceneStorageFixture _fixture;

        public LuceneTests(LuceneStorageFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void AddDocument_WhenCalled_ShouldNotThrow()
        {
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

            _fixture.Writer.AddDocument(doc);

            _fixture.Writer.Flush(triggerMerge: false, applyAllDeletes: false);
        }

        [Fact]
        public void Fetch_WhenCalled_ShouldReturnSomeHits()
        {
            var phrase = new MultiPhraseQuery
            {
                new Term("favoritePhrase", "brown"),
                new Term("favoritePhrase", "fox")
            };

            var searcher = _fixture.CreateSearcher();

            var hits = searcher.Search(phrase, 20).ScoreDocs;

            Assert.NotEmpty(hits);
        }
    }
}
