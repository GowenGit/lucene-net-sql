using Lucene.Net.Sql.Operators;
using Moq;
using Xunit;

namespace Lucene.Net.Sql.Tests
{
    public class SqlIndexOutputTests
    {
        private const string UnitTest = "test";
        private const int UnitTestBlockSize = 4;

        private static SqlIndexOutput GetSutObject(IOperator sqlOperator)
        {
            var options = new SqlDirectoryOptions(
                SqlDirectoryEngine.MySql,
                UnitTest,
                UnitTest)
            { BlockSize = UnitTestBlockSize };

            return new SqlIndexOutput(options, sqlOperator, UnitTest);
        }

        [Fact]
        public void Write_OneByte_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            using var sut = GetSutObject(sqlOperator.Object);

            sut.WriteByte(1);

            sut.Flush();

            sqlOperator.Verify(x => x.WriteBlock(UnitTest, 0, VerifyArray(new byte[] { 1 })));

            Assert.Equal(1, sut.Length);
        }

        [Fact]
        public void Write_MultipleBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 1, VerifyArray(new byte[] { 2, 1, 1, 1 }))).Verifiable();

            using var sut = GetSutObject(sqlOperator.Object);

            sut.WriteByte(1);
            sut.WriteByte(1);
            sut.WriteByte(1);
            sut.WriteByte(1);

            sut.WriteByte(2);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(5, sut.Length);
        }

        private static byte[] VerifyArray(byte[] expected)
        {
            return It.Is<byte[]>(actual => VerifyArray(expected, actual));
        }

        private static bool VerifyArray(byte[] expected, byte[] actual)
        {
            Assert.Equal(UnitTestBlockSize, actual.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }

            return true;
        }
    }
}
