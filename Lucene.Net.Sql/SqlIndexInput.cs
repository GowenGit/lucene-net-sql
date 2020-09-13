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
        private readonly long _nodeId;

        /// <summary>
        /// Gets file length.
        /// </summary>
        public override long Length { get; }

        private long _pos;
        private long? _block;

        internal SqlIndexInput(SqlDirectoryOptions options, IOperator sqlOperator, string name) : base(name)
        {
            var node = sqlOperator.GetNode(name);

            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            _sqlOperator = sqlOperator;
            _bufferSize = options.BlockSize;
            _buffer = new byte[_bufferSize];
            _name = name;

            _nodeId = node.Id;
            Length = node.Size;
        }

        private SqlIndexInput(IOperator sqlOperator, string name, int bufferSize, long nodeId, long length, byte[] buffer) : base(name)
        {
            _sqlOperator = sqlOperator;
            _bufferSize = bufferSize;
            _buffer = buffer;
            _name = name;

            _nodeId = nodeId;
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
                throw new EndOfStreamException($"Read past EOF: {_nodeId}, pos {_block * _bufferSize}");
            }

            var blockBuffer = _sqlOperator.GetBlock(_nodeId, block);

            Buffer.BlockCopy(blockBuffer, 0, _buffer, 0, blockBuffer.Length);

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
            var buffer = _buffer.ToArray();

            var clone = new SqlIndexInput(_sqlOperator, _name, _bufferSize, _nodeId, Length, buffer);

            clone._pos = _pos;
            clone._block = _block;

            return clone;
        }

        protected override void Dispose(bool disposing) { }
    }
}