using System;
using Lucene.Net.Sql.Exceptions;
using Lucene.Net.Sql.Models;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Lucene.Net.Sql
{
    internal class SqlIndexInput : BufferedIndexInput
    {
        private readonly IDatabaseLuceneOperator _sqlOperator;

        private readonly Node _node;

        private readonly long _offset;
        private readonly long _end;

        private readonly int _bufferSize;
        private byte[] _buffer;
        private long? _block;

        public sealed override long Length => _end - _offset;

        internal SqlIndexInput(
            SqlDirectoryOptions options,
            IDatabaseLuceneOperator sqlOperator,
            Node node,
            IOContext context) : base(node.Name, context)
        {
            _sqlOperator = sqlOperator;
            _node = node;
            _bufferSize = options.BlockSize;
            _buffer = new byte[_bufferSize];

            _node = node;

            _offset = 0L;
            _end = node.Size;
        }

        internal SqlIndexInput(
            SqlDirectoryOptions options,
            IDatabaseLuceneOperator sqlOperator,
            Node node,
            long offset,
            long length,
            int bufferSize) : base(node.Name, bufferSize)
        {
            _sqlOperator = sqlOperator;
            _node = node;
            _bufferSize = options.BlockSize;
            _buffer = new byte[_bufferSize];

            _node = node;

            _offset = offset;
            _end = offset + length;
        }

        private void FetchBlock(long block)
        {
            if (_block == block)
            {
                return;
            }

            DebugLogger.LogReader($"{this} Fetching block: {block}");

            _sqlOperator.GetBlock(_node.Id, block, _buffer, 0, 0, _bufferSize);

            _block = block;
        }

        protected override void ReadInternal(byte[] b, int offset, int length)
        {
            var position = _offset + GetFilePointer();

            DebugLogger.LogReader($"{this} Reading offset: {offset}, length: {length}, position: {position}, current block: {_block}");

            if (position + length > _end)
            {
                throw new LuceneSqlException($"Read past EOF: {_node.Id}, pos {position}");
            }

            for (var i = position / _bufferSize; i <= (position + length) / _bufferSize; i++)
            {
                FetchBlock(i);

                var rangeStart = Math.Max(position, i * _bufferSize) - i * _bufferSize;
                var rangeEnd = Math.Min(position + length, (i + 1) * _bufferSize) - i * _bufferSize;

                var len = rangeEnd - rangeStart;

                Array.Copy(_buffer, rangeStart, b, offset, len);

                offset += (int)len;
            }
        }

        protected override void SeekInternal(long pos) { }

        protected override void Dispose(bool disposing) { }

        public override object Clone()
        {
            DebugLogger.LogReader($"Cloning {this}");

            var clone = (SqlIndexInput)base.Clone();

            var buffer = new byte[_bufferSize];

            Array.Copy(_buffer, buffer, _bufferSize);

            clone._buffer = buffer;
            clone._block = _block;

            return clone;
        }

        public override string ToString()
        {
            return $"Node: {_node.Id} [{_node.Name}] [{_offset}:{_end}]";
        }
    }

    internal class SqlIndexInputSlicer : Directory.IndexInputSlicer
    {
        private readonly SqlDirectoryOptions _options;
        private readonly IDatabaseLuceneOperator _sqlOperator;
        private readonly Node _node;
        private readonly IOContext _context;

        public SqlIndexInputSlicer(
            SqlDirectoryOptions options,
            IDatabaseLuceneOperator sqlOperator,
            Node node,
            IOContext context)
        {
            _options = options;
            _sqlOperator = sqlOperator;
            _node = node;
            _context = context;
        }

        public override IndexInput OpenSlice(string sliceDescription, long offset, long length)
        {
            var description =
                $"SqlIndexInput({sliceDescription} in path=\"{_node.Name}\" slice={offset}:{offset + length})";

            var node = new Node
            {
                Id = _node.Id,
                Name = description,
                Size = _node.Size
            };

            return new SqlIndexInput(
                _options,
                _sqlOperator,
                node,
                offset,
                length,
                BufferedIndexInput.GetBufferSize(_context));
        }

        [Obsolete("Only for reading CFS files from 3.x indexes.")]
        public override IndexInput OpenFullSlice()
        {
            try
            {
                return OpenSlice("full-slice", 0, _node.Size);
            }
            catch (Exception ex)
            {
                throw new LuceneSqlException("Failed to open full slice", ex);
            }
        }

        protected override void Dispose(bool disposing) { }
    }
}