﻿using Lucene.Net.Sql.Operators;
using Moq;
using Xunit;

#pragma warning disable CS0618

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

        [Fact]
        public void Write_BatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 1, VerifyArray(new byte[] { 2, 2, 2, 2 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 2, VerifyArray(new byte[] { 3, 3, 3, 3 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 3, VerifyArray(new byte[] { 4, 4, 4, 4 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 4, VerifyArray(new byte[] { 5, 5, 5, 5 }))).Verifiable();

            using var sut = GetSutObject(sqlOperator.Object);

            sut.WriteBytes(new byte[] {1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5}, 20);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(20, sut.Length);
        }

        [Fact]
        public void Write_OverrideBatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 1, VerifyArray(new byte[] { 2, 2, 2, 2 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 2, VerifyArray(new byte[] { 3, 3, 3, 3 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 3, VerifyArray(new byte[] { 4, 4, 4, 4 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 4, VerifyArray(new byte[] { 5, 5, 5, 5 }))).Verifiable();

            using var sut = GetSutObject(sqlOperator.Object);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5 }, 20);

            sut.Seek(3);

            sqlOperator.Setup(x => x.GetBlock(UnitTest, 0)).Returns(new byte[] {1, 1, 1, 1});
            sqlOperator.Setup(x => x.GetBlock(UnitTest, 1)).Returns(new byte[] { 2, 2, 2, 2 });
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 0, VerifyArray(new byte[] { 1, 1, 1, 6 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 1, VerifyArray(new byte[] { 6, 6, 2, 2 }))).Verifiable();

            sut.WriteBytes(new byte[] { 6, 6, 6 }, 3);

            sut.Flush();

            sqlOperator.Verify();

            Assert.Equal(20, sut.Length);
        }

        [Fact]
        public void Write_PadBatchBytes_ShouldFlush()
        {
            var sqlOperator = new Mock<IOperator>();

            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 1, VerifyArray(new byte[] { 2, 2, 2, 2 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 2, VerifyArray(new byte[] { 3, 3, 3, 3 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 3, VerifyArray(new byte[] { 4, 4, 4, 4 }))).Verifiable();

            using var sut = GetSutObject(sqlOperator.Object);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4 }, 16);

            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 4, VerifyArray(new byte[] { 0, 0, 0, 0 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 5, VerifyArray(new byte[] { 8, 0, 0, 0 }))).Verifiable();

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

            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 0, VerifyArray(new byte[] { 1, 1, 1, 1 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 1, VerifyArray(new byte[] { 2, 2, 2, 2 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 2, VerifyArray(new byte[] { 3, 3, 3, 3 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 3, VerifyArray(new byte[] { 4, 4, 4, 4 }))).Verifiable();

            using var sut = GetSutObject(sqlOperator.Object);

            sut.WriteBytes(new byte[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4 }, 16);

            sqlOperator.Setup(x => x.GetBlock(UnitTest, 1)).Returns(new byte[] { 2, 2, 2, 2 });
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 1, VerifyArray(new byte[] { 6, 6, 6, 2 }))).Verifiable();

            sut.Seek(4);

            sut.WriteBytes(new byte[] { 6, 6, 6 }, 3);

            sqlOperator.Setup(x => x.GetBlock(UnitTest, 4)).Returns(new byte[] { 0, 0, 0, 2 });
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 4, VerifyArray(new byte[] { 0, 0, 0, 2 }))).Verifiable();

            sut.Seek(19);

            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 4, VerifyArray(new byte[] { 0, 0, 0, 9 }))).Verifiable();
            sqlOperator.Setup(x => x.WriteBlock(UnitTest, 5, VerifyArray(new byte[] { 9, 9, 9, 2 }))).Verifiable();

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
