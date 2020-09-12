using System;
using Lucene.Net.Sql.Operators;
using Lucene.Net.Store;

#pragma warning disable SA1018

namespace Lucene.Net.Sql
{
    internal class SqlIndexOutput : IndexOutput
    {
        private readonly IChecksum _checksum;
        private readonly int _bufferSize;

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

        internal SqlIndexOutput(SqlDirectoryOptions options, IOperator sqlOperator, string name)
        {
            _checksum = new Checksum();
            _bufferSize = options.BlockSize;
            _length = sqlOperator.GetNode(name)?.Size ?? 0;
        }

        public override void WriteByte(byte b)
        {
            if (_buffer == null)
            {
                _buffer = new byte[_bufferSize];
            }

            FlushIfFull();

            _buffer[_bufferPosition++] = b;

            _length++;

            _checksum.Update(b);
        }

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
        /// Or if current buffer that we have is full.
        /// </summary>
        private void FlushIfFull()
        {
            if ((_bufferStart + _bufferPosition) / _bufferSize > _length / _bufferSize)
            {
                // Handles all append
                // cases.
                Flush();
            }
            else if (_bufferPosition >= _bufferSize)
            {
                // Can happen if we update
                // in the middle of the file
                Flush();
            }
        }

        public override void Flush()
        {
            if (_buffer == null)
            {
                return;
            }

            // TODO: write data
            _bufferStart += _bufferPosition;
            _bufferPosition = 0;
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
                var pad = pos - _length;

                WriteBytes(new byte[pad], 0, (int)pad);
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