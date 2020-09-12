using System;
using System.IO;
using System.Linq;
using Lucene.Net.Sql.Operators;
using Lucene.Net.Store;

#pragma warning disable CA2000

namespace Lucene.Net.Sql
{
    internal class SqlIndexInput : IndexInput
    {
        private readonly IOperator _sqlOperator;

        private readonly string _name;
        private readonly int _bufferSize;

        private readonly byte[] _buffer;

        /// <summary>
        /// Gets file length.
        /// </summary>
        public override long Length { get; }

        private long _pos;
        private long? _block;

        internal SqlIndexInput(SqlDirectoryOptions options, IOperator sqlOperator, string name) : base(name)
        {
            _sqlOperator = sqlOperator;
            _name = name;
            _bufferSize = options.BlockSize;
            _buffer = new byte[_bufferSize];

            Length = sqlOperator.GetNode(name)?.Size ?? 0;
        }

        private SqlIndexInput(IOperator sqlOperator, string name, int bufferSize, long length, byte[] buffer) : base(name)
        {
            _sqlOperator = sqlOperator;
            _name = name;
            _bufferSize = bufferSize;
            _buffer = buffer;

            Length = length;
        }

        public override byte ReadByte()
        {
            var block = _pos / _bufferSize;

            if (block != _block)
            {
                FetchBlock(block);
            }

            return _buffer[_pos++ % _bufferSize];
        }

        private void FetchBlock(long block)
        {
            if (_block * _bufferSize > Length)
            {
                throw new EndOfStreamException($"Read past EOF: {_name}, pos {_block * _bufferSize}");
            }

            var blockBuffer = _sqlOperator.GetBlock(_name, block);

            Buffer.BlockCopy(blockBuffer, 0, _buffer, 0, blockBuffer.Length);

            _block = block;
        }

        public override void ReadBytes(byte[] b, int offset, int len)
        {
            for (var i = 0; i < len; i++)
            {
                b[offset + i] = ReadByte();
            }
        }

        public override long GetFilePointer()
        {
            return _pos;
        }

        public override void Seek(long pos)
        {
            _pos = pos;
        }

        public override object Clone()
        {
            var buffer = _buffer.ToArray();

            var clone = new SqlIndexInput(_sqlOperator, _name, _bufferSize, Length, buffer)
            {
                _pos = _pos,
                _block = _block
            };

            return clone;
        }

        protected override void Dispose(bool disposing) { }
    }
}