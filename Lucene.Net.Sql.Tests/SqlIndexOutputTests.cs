using Lucene.Net.Sql.Models;
using Lucene.Net.Sql.Operators;
using Moq;
using Xunit;

#pragma warning disable CS0618

namespace Lucene.Net.Sql.Tests
{
    public class SqlIndexOutputTests
    {
        private const long UnitTestId = 1;
        private const string UnitTest = "test";

        private const int UnitTestBlockSize = 4;

        private static SqlIndexOutput GetSutObject(Mock<IOperator> sqlOperator)
        {
            sqlOperator
                .Setup(x => x.GetNode(It.IsAny<string>()))
                .Returns(new Node {Id = UnitTestId});

            var options = new SqlDirectoryOptions(
                SqlDirectoryEngine.MySql,
                UnitTest,
                UnitTest)
            { BlockSize = UnitTestBlockSize };

            return new SqlIndexOutput(options, sqlOperator.Object, UnitTest);
        }

        [Fact]
        public void Write_OneByte_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            using var sut = GetSutObject(sqlOperator);

            sut.WriteByte(1);

            sut.Flush();

            sqlOperator.Verify(x => x.WriteBlock(UnitTestId, 0, VerifyArray(new byte[] { 1 }), 1));

            Assert.Equal(1, sut.Length);
        }

        [Fact]
        public void Write_MultipleBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }), 4)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 1, VerifyArray(new byte[] { 2, 1, 1, 1 }), 5)).Verifiable();

            using var sut = GetSutObject(sqlOperator);

            sut.WriteByte(1);
            sut.WriteByte(1);
            sut.WriteByte(1);
            sut.WriteByte(1);

            sut.WriteByte(2);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(5, sut.Length);
        }

        [Fact]
        public void Write_BatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }), 4)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 1, VerifyArray(new byte[] { 2, 2, 2, 2 }), 8)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 2, VerifyArray(new byte[] { 3, 3, 3, 3 }), 12)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 3, VerifyArray(new byte[] { 4, 4, 4, 4 }), 16)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 4, VerifyArray(new byte[] { 5, 5, 5, 5 }), 20)).Verifiable();

            using var sut = GetSutObject(sqlOperator);

            sut.WriteBytes(new byte[] {1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5}, 20);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(20, sut.Length);
        }

        [Fact]
        public void Write_OverrideBatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }), 4)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 1, VerifyArray(new byte[] { 2, 2, 2, 2 }), 8)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 2, VerifyArray(new byte[] { 3, 3, 3, 3 }), 12)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 3, VerifyArray(new byte[] { 4, 4, 4, 4 }), 16)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 4, VerifyArray(new byte[] { 5, 5, 5, 5 }), 20)).Verifiable();

            using var sut = GetSutObject(sqlOperator);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5 }, 20);

            sut.Seek(3);

            sqlOperator.Setup(x => x.GetBlock(UnitTestId, 0)).Returns(new byte[] {1, 1, 1, 1});
            sqlOperator.Setup(x => x.GetBlock(UnitTestId, 1)).Returns(new byte[] { 2, 2, 2, 2 });
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 0, VerifyArray(new byte[] { 1, 1, 1, 6 }), 20)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 1, VerifyArray(new byte[] { 6, 6, 2, 2 }), 20)).Verifiable();

            sut.WriteBytes(new byte[] { 6, 6, 6 }, 3);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(20, sut.Length);
        }

        [Fact]
        public void Write_PadBatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }), 4)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 1, VerifyArray(new byte[] { 2, 2, 2, 2 }), 8)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 2, VerifyArray(new byte[] { 3, 3, 3, 3 }), 12)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 3, VerifyArray(new byte[] { 4, 4, 4, 4 }), 16)).Verifiable();

            using var sut = GetSutObject(sqlOperator);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4 }, 16);

            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 4, VerifyArray(new byte[] { 0, 0, 0, 0 }), 20)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 5, VerifyArray(new byte[] { 8, 0, 0, 0 }), 21)).Verifiable();

            sut.Seek(20);

            sut.WriteByte(8);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(21, sut.Length);
        }

        [Fact]
        public void Write_SeekWriteBatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }), 4)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 1, VerifyArray(new byte[] { 2, 2, 2, 2 }), 8)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 2, VerifyArray(new byte[] { 3, 3, 3, 3 }), 12)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 3, VerifyArray(new byte[] { 4, 4, 4, 4 }), 16)).Verifiable();

            using var sut = GetSutObject(sqlOperator);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4 }, 16);

            sqlOperator.Setup(x => x.GetBlock(UnitTestId, 1)).Returns(new byte[] { 2, 2, 2, 2 });
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 1, VerifyArray(new byte[] { 6, 6, 6, 2 }), 16)).Verifiable();

            sut.Seek(4);

            sut.WriteBytes(new byte[] { 6, 6, 6 }, 3);

            sqlOperator.Setup(x => x.GetBlock(UnitTestId, 4)).Returns(new byte[] { 0, 0, 0, 2 });
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 4, VerifyArray(new byte[] { 0, 0, 0, 2 }), 19)).Verifiable();

            sut.Seek(19);

            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 4, VerifyArray(new byte[] { 0, 0, 0, 9 }), 20)).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, 5, VerifyArray(new byte[] { 9, 9, 9, 2 }), 23)).Verifiable();

            sut.WriteBytes(new byte[] { 9, 9, 9, 9 }, 4);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(23, sut.Length);
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
