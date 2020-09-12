using System;
using Lucene.Net.Sql.Operators;
using Lucene.Net.Store;

#pragma warning disable SA1018

namespace Lucene.Net.Sql
{
    internal class SqlIndexOutput : IndexOutput
    {
        private readonly IOperator _sqlOperator;
        private readonly IChecksum _checksum;

        private readonly string _name;
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
            _sqlOperator = sqlOperator;
            _name = name;
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
        /// </summary>
        private void FlushIfFull()
        {
            if ((_bufferStart + _bufferPosition) / _bufferSize > _length / _bufferSize)
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

            if (_bufferStart % _bufferSize == 0)
            {
                // not full block and not at the end of the file
                if (_bufferPosition != _bufferSize && _bufferStart + _bufferPosition < Length)
                {
                    var firstBlock = _sqlOperator.GetBlock(_name, block);

                    Buffer.BlockCopy(firstBlock, _bufferPosition, _buffer, _bufferPosition, firstBlock.Length - _bufferPosition);
                }

                _sqlOperator.WriteBlock(_name, block, _buffer);
            }
            else
            {
                var firstBuffer = new byte[_bufferSize];

                var firstBlock = _sqlOperator.GetBlock(_name, block);

                Buffer.BlockCopy(firstBlock, 0, firstBuffer, 0, (int)(_bufferStart % _bufferSize));
                Buffer.BlockCopy(_buffer, 0, firstBuffer, (int)(_bufferStart % _bufferSize), _bufferPosition);

                _sqlOperator.WriteBlock(_name, block, firstBuffer);
            }

            _bufferStart += _bufferPosition;
            _bufferPosition = 0;
            _length = Math.Max(_length, _bufferStart);
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