using System;
using Lucene.Net.Sql.Models;
using Moq;
using Xunit;

#pragma warning disable CS0618

// ReSharper disable SuggestBaseTypeForParameter
namespace Lucene.Net.Sql.Tests
{
    public class SqlIndexOutputTests
    {
        private const long UnitTestId = 1;
        private const string UnitTest = "test";

        private const int UnitTestBlockSize = 4;

        private static SqlIndexOutput GetSutObject(Mock<IDatabaseLuceneOperator> sqlOperator)
        {
            var options = new SqlDirectoryOptions(
                UnitTest,
                UnitTest)
            { BlockSize = UnitTestBlockSize };

            return new SqlIndexOutput(options, sqlOperator.Object, new Node { Id = UnitTestId, Name = UnitTest });
        }

        [Fact]
        public void Write_OneByte_ShouldFlush()
        {
            var sqlOperator = new Mock<IDatabaseLuceneOperator>();

            using var sut = GetSutObject(sqlOperator);

            SetupWriteBlock(sqlOperator, 0, new byte[] { 1 }, 1);

            sut.WriteByte(1);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(1, sut.Length);
        }

        [Fact]
        public void Write_MultipleBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IDatabaseLuceneOperator>();

            SetupWriteBlock(sqlOperator, 0, new byte[] { 1, 1, 1, 1 }, 4);
            SetupWriteBlock(sqlOperator, 1, new byte[] { 2, 1, 1, 1 }, 5);

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
            var sqlOperator = new Mock<IDatabaseLuceneOperator>();

            SetupWriteBlock(sqlOperator, 0, new byte[] { 1, 1, 1, 1 }, 4);
            SetupWriteBlock(sqlOperator, 1, new byte[] { 2, 2, 2, 2 }, 8);
            SetupWriteBlock(sqlOperator, 2, new byte[] { 3, 3, 3, 3 }, 12);
            SetupWriteBlock(sqlOperator, 3, new byte[] { 4, 4, 4, 4 }, 16);
            SetupWriteBlock(sqlOperator, 4, new byte[] { 5, 5, 5, 5 }, 20);

            using var sut = GetSutObject(sqlOperator);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5 }, 20);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(20, sut.Length);
        }

        [Fact]
        public void Write_OverrideBatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IDatabaseLuceneOperator>();

            SetupWriteBlock(sqlOperator, 0, new byte[] { 1, 1, 1, 1 }, 4);
            SetupWriteBlock(sqlOperator, 1, new byte[] { 2, 2, 2, 2 }, 8);
            SetupWriteBlock(sqlOperator, 2, new byte[] { 3, 3, 3, 3 }, 12);
            SetupWriteBlock(sqlOperator, 3, new byte[] { 4, 4, 4, 4 }, 16);
            SetupWriteBlock(sqlOperator, 4, new byte[] { 5, 5, 5, 5 }, 20);

            using var sut = GetSutObject(sqlOperator);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5 }, 20);

            sut.Seek(3);

            SetupGetBlock(sqlOperator, 0, new byte[] { 1, 1, 1, 1 });
            SetupGetBlock(sqlOperator, 1, new byte[] { 2, 2, 2, 2 });
            SetupWriteBlock(sqlOperator, 0, new byte[] { 1, 1, 1, 6 }, 20);
            SetupWriteBlock(sqlOperator, 1, new byte[] { 6, 6, 2, 2 }, 20);

            sut.WriteBytes(new byte[] { 6, 6, 6 }, 3);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(20, sut.Length);
        }

        [Fact]
        public void Write_PadBatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IDatabaseLuceneOperator>();

            SetupWriteBlock(sqlOperator, 0, new byte[] { 1, 1, 1, 1 }, 4);
            SetupWriteBlock(sqlOperator, 1, new byte[] { 2, 2, 2, 2 }, 8);
            SetupWriteBlock(sqlOperator, 2, new byte[] { 3, 3, 3, 3 }, 12);
            SetupWriteBlock(sqlOperator, 3, new byte[] { 4, 4, 4, 4 }, 16);

            using var sut = GetSutObject(sqlOperator);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4 }, 16);

            SetupWriteBlock(sqlOperator, 4, new byte[] { 0, 0, 0, 0 }, 20);
            SetupWriteBlock(sqlOperator, 5, new byte[] { 8, 0, 0, 0 }, 21);

            sut.Seek(20);

            sut.WriteByte(8);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(21, sut.Length);
        }

        [Fact]
        public void Write_SeekWriteBatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IDatabaseLuceneOperator>();

            SetupWriteBlock(sqlOperator, 0, new byte[] { 1, 1, 1, 1 }, 4);
            SetupWriteBlock(sqlOperator, 1, new byte[] { 2, 2, 2, 2 }, 8);
            SetupWriteBlock(sqlOperator, 2, new byte[] { 3, 3, 3, 3 }, 12);
            SetupWriteBlock(sqlOperator, 3, new byte[] { 4, 4, 4, 4 }, 16);

            using var sut = GetSutObject(sqlOperator);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4 }, 16);

            SetupGetBlock(sqlOperator, 1, new byte[] { 2, 2, 2, 2 });
            SetupWriteBlock(sqlOperator, 1, new byte[] { 6, 6, 6, 2 }, 16);

            sut.Seek(4);

            sut.WriteBytes(new byte[] { 6, 6, 6 }, 3);

            SetupGetBlock(sqlOperator, 4, new byte[] { 0, 0, 0, 2 });
            SetupWriteBlock(sqlOperator, 4, new byte[] { 0, 0, 0, 2 }, 19);

            sut.Seek(19);

            SetupWriteBlock(sqlOperator, 4, new byte[] { 0, 0, 0, 9 }, 20);
            SetupWriteBlock(sqlOperator, 5, new byte[] { 9, 9, 9, 2 }, 23);

            sut.WriteBytes(new byte[] { 9, 9, 9, 9 }, 4);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(23, sut.Length);
        }

        private static void SetupWriteBlock(Mock<IDatabaseLuceneOperator> sqlOperator, long block, byte[] array, long nodeLength)
        {
            sqlOperator.Setup(x => x.WriteBlock(UnitTestId, block, VerifyArray(array), nodeLength)).Verifiable();
        }

        private static void SetupGetBlock(Mock<IDatabaseLuceneOperator> sqlOperator, long block, byte[] array)
        {
            sqlOperator
                .Setup(x => x.GetBlock(UnitTestId, block, It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<long, long, byte[], int, int, int>((id, blockId, buffer, srcOffset, dstOffset, length) =>
                {
                    Array.Copy(array, srcOffset, buffer, dstOffset, length);
                });
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
