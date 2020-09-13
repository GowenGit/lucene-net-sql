using System;
using Lucene.Net.Sql.Exceptions;
using Lucene.Net.Sql.Models;
using Lucene.Net.Store;

#pragma warning disable CA2000

namespace Lucene.Net.Sql
{
    internal class SqlIndexInput : IndexInput
    {
        private readonly IDatabaseLuceneOperator _sqlOperator;

        private readonly Node _node;

        private readonly int _bufferSize;
        private readonly byte[] _buffer;

        /// <summary>
        /// Gets file length.
        /// </summary>
        public override long Length { get; }

        private long _pos;
        private long? _block;

        internal SqlIndexInput(
            SqlDirectoryOptions options,
            IDatabaseLuceneOperator sqlOperator,
            Node node) : base(node.Name)
        {
            _sqlOperator = sqlOperator;
            _node = node;
            _bufferSize = options.BlockSize;
            _buffer = new byte[_bufferSize];

            _node = node;

            Length = node.Size;
        }

        private SqlIndexInput(
            IDatabaseLuceneOperator sqlOperator,
            Node node,
            int bufferSize,
            byte[] buffer) : base(node.Name)
        {
            _sqlOperator = sqlOperator;
            _bufferSize = bufferSize;
            _buffer = buffer;

            _node = node;
            Length = node.Size;
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

        /// <summary>
        /// TODO: Array.Copy optimize.
        /// </summary>
        public override void ReadBytes(byte[] b, int offset, int len)
        {
            for (var i = 0; i < len; i++)
            {
                b[offset + i] = ReadByte();
            }
        }

        private void FetchBlock(long block)
        {
            if (_block * _bufferSize > Length)
            {
                throw new LuceneSqlException($"Read past EOF: {_node.Id}, pos {_block * _bufferSize}");
            }

            _sqlOperator.GetBlock(_node.Id, block, _buffer, 0, 0, _bufferSize);

            _block = block;
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
            var buffer = new byte[_bufferSize];

            Array.Copy(_buffer, buffer, _bufferSize);

            var clone = new SqlIndexInput(_sqlOperator, _node, _bufferSize, buffer)
            {
                _pos = _pos,
                _block = _block
            };

            return clone;
        }

        protected override void Dispose(bool disposing) { }
    }
}