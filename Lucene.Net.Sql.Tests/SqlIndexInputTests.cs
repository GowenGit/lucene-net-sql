using System;
using Lucene.Net.Sql.Models;
using Moq;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]

// ReSharper disable SuggestBaseTypeForParameter
namespace Lucene.Net.Sql.Tests
{
    public class SqlIndexInputTests
    {
        private const string UnitTest = "test";
        private const long UnitTestId = 0;

        private const int UnitTestBlockSize = 3;

        private static SqlIndexInput GetSutObject()
        {
            var sqlOperator = new Mock<IDatabaseLuceneOperator>();

            SetupGetBlock(sqlOperator, 0, new byte[] { 1, 1, 1 });
            SetupGetBlock(sqlOperator, 1, new byte[] { 2, 2, 2 });
            SetupGetBlock(sqlOperator, 2, new byte[] { 3, 3, 3 });
            SetupGetBlock(sqlOperator, 3, new byte[] { 4, 3, 3 });

            var options = new SqlDirectoryOptions(
                    UnitTest,
                    UnitTest)
            { BlockSize = UnitTestBlockSize };

            return new SqlIndexInput(options, sqlOperator.Object, new Node { Size = 10, Name = UnitTest, Id = UnitTestId}, 0, 10, 3);
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

            VerifyArray(new byte[] { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4 }, actual);
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

        private static void SetupGetBlock(Mock<IDatabaseLuceneOperator> sqlOperator, long block, byte[] array)
        {
            sqlOperator
                .Setup(x => x.GetBlock(UnitTestId, block, It.IsAny<byte[]>(), 0, 0, UnitTestBlockSize))
                .Callback<long, long, byte[], int, int, int>((id, blockId, buffer, srcOffset, dstOffset, length) =>
                {
                    Array.Copy(array, srcOffset, buffer, dstOffset, length);
                });
        }

        private static void VerifyArray(byte[] expected, byte[] actual)
        {
            Assert.Equal(expected.Length, actual.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }
    }
}
