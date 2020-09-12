using Lucene.Net.Sql.Models;
using Lucene.Net.Sql.Operators;
using Moq;
using Xunit;

namespace Lucene.Net.Sql.Tests
{
    public class SqlIndexInputTests
    {
        private const string UnitTest = "test";
        private const int UnitTestBlockSize = 3;

        private static SqlIndexInput GetSutObject()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator
                .Setup(x => x.GetNode(UnitTest))
                .Returns(new Node { Size = 10 });

            sqlOperator
                .Setup(x => x.GetBlock(UnitTest, 0))
                .Returns(new byte[] {1, 1, 1});

            sqlOperator
                .Setup(x => x.GetBlock(UnitTest, 1))
                .Returns(new byte[] { 2, 2, 2 });

            sqlOperator
                .Setup(x => x.GetBlock(UnitTest, 2))
                .Returns(new byte[] { 3, 3, 3 });

            sqlOperator
                .Setup(x => x.GetBlock(UnitTest, 3))
                .Returns(new byte[] { 4, 3, 3 });

            var options = new SqlDirectoryOptions(
                    SqlDirectoryEngine.MySql,
                    UnitTest,
                    UnitTest)
                { BlockSize = UnitTestBlockSize };

            return new SqlIndexInput(options, sqlOperator.Object, UnitTest);
        }

        [Fact]
        public void Read_OneAtATime_ShouldSucceed()
        {
            using var sut = GetSutObject();

            var actual = string.Empty;

            for (var i = 0; i < sut.Length; i++)
            {
                actual += sut.ReadByte();
            }

            Assert.Equal("1112223334", actual);
        }

        [Fact]
        public void Read_AllBatch_ShouldSucceed()
        {
            using var sut = GetSutObject();

            var actual = new byte[10];

            sut.ReadBytes(actual, 0, 10);

            VerifyArray(new byte[] { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4}, actual);
        }

        [Fact]
        public void Read_OffsetBatch_ShouldSucceed()
        {
            using var sut = GetSutObject();

            var actual = new byte[10];

            sut.ReadBytes(actual, 5, 5);

            VerifyArray(new byte[] { 0, 0, 0, 0, 0, 1, 1, 1, 2, 2 }, actual);
        }

        [Fact]
        public void Read_SeekBatch_ShouldSucceed()
        {
            using var sut = GetSutObject();

            var actual = new byte[4];

            sut.Seek(3);

            sut.ReadBytes(actual, 0, 4);

            VerifyArray(new byte[] { 2, 2, 2, 3 }, actual);
        }

        private static bool VerifyArray(byte[] expected, byte[] actual)
        {
            Assert.Equal(expected.Length, actual.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }

            return true;
        }
    }
}
