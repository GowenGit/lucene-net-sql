using System;
using Lucene.Net.Sql.Models;
using Lucene.Net.Store;

#pragma warning disable SA1018

namespace Lucene.Net.Sql
{
    internal class SqlIndexOutput : IndexOutput
    {
        private readonly IDatabaseLuceneOperator _sqlOperator;
        private readonly IChecksum _checksum;

        private readonly int _bufferSize;
        private readonly long _nodeId;

        /// <summary>
        /// Gets the current checksum of bytes written so far.
        /// </summary>
        public override long Checksum => _checksum.Value;

        /// <summary>
        /// Gets file length.
        /// </summary>
        public override long Length => _length;

        private long _bufferStart;
        private int _bufferPosition;
        private long _length;

        private byte[] ? _buffer;

        internal SqlIndexOutput(
            SqlDirectoryOptions options,
            IDatabaseLuceneOperator sqlOperator,
            Node node)
        {
            _sqlOperator = sqlOperator;
            _checksum = new Checksum();
            _bufferSize = options.BlockSize;

            _nodeId = node.Id;
            _length = node.Size;
        }

        public override void WriteByte(byte b)
        {
            if (_buffer == null)
            {
                _buffer = new byte[_bufferSize];
            }

            FlushIfFull();

            _buffer[_bufferPosition++] = b;

            _checksum.Update(b);
        }

        /// <summary>
        /// TODO: Array.Copy optimize.
        /// </summary>
        public override void WriteBytes(byte[] b, int offset, int length)
        {
            if (_buffer == null)
            {
                _buffer = new byte[_bufferSize];
            }

            // Maybe we want to be more
            // clever here in the future with
            // batching checksum calculation
            // _checksum.Update(b, offset, length);
            for (var i = offset; i < length; i++)
            {
                WriteByte(b[i]);
            }
        }

        /// <summary>
        /// Flush if we currently can populate fully
        /// more data blocks than before.
        /// </summary>
        private void FlushIfFull()
        {
            if ((_bufferStart + _bufferPosition) % _bufferSize == 0)
            {
                Flush();
            }
        }

        public override void Flush()
        {
            if (_buffer == null || _bufferPosition == 0)
            {
                return;
            }

            var block = _bufferStart / _bufferSize;

            byte[] buffer;

            if (_bufferStart % _bufferSize == 0)
            {
                // not full block and not at the end of the file, fill in remaining spaces from stored block.
                if (_bufferPosition != _bufferSize && _bufferStart + _bufferPosition < Length)
                {
                    _sqlOperator.GetBlock(_nodeId, block, _buffer, _bufferPosition, _bufferPosition, _bufferSize - _bufferPosition);
                }

                buffer = _buffer;
            }
            else
            {
                buffer = new byte[_bufferSize];

                // fill in start of the block with stored buffer data
                _sqlOperator.GetBlock(_nodeId, block, buffer, 0, 0, (int)(_bufferStart % _bufferSize));

                // fill in remainder of the block with leftover data
                Array.Copy(_buffer, 0, buffer, (int)(_bufferStart % _bufferSize), _bufferPosition);
            }

            _bufferStart += _bufferPosition;
            _bufferPosition = 0;
            _length = Math.Max(_length, _bufferStart);

            _sqlOperator.WriteBlock(_nodeId, block, buffer, _length);
        }

        public override long GetFilePointer()
        {
            return _bufferStart + _bufferPosition;
        }

        [Obsolete("(4.1) this method will be removed in Lucene 5.0")]
        public override void Seek(long pos)
        {
            Flush();

            if (pos > _length)
            {
                // make sure we append to the end
                _bufferStart = _length;

                var pad = pos - _length;

                WriteBytes(new byte[pad], 0, (int)pad);

                Flush();
            }

            _bufferStart = pos;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Flush();
        }
    }
}