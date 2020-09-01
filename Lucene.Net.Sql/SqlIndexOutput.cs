using System;
using Lucene.Net.Store;

namespace Lucene.Net.Sql
{
    internal class SqlIndexInput : IndexInput
    {
        /// <summary>
        /// Gets file length.
        /// </summary>
        public override long Length { get; }

        public SqlIndexInput(string name) : base(name)
        {
            Length = 0;
        }

        public override byte ReadByte()
        {
            throw new System.NotImplementedException();
        }

        public override void ReadBytes(byte[] b, int offset, int len)
        {
            throw new System.NotImplementedException();
        }

        public override long GetFilePointer()
        {
            throw new System.NotImplementedException();
        }

        public override void Seek(long pos)
        {
            throw new System.NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new System.NotImplementedException();
        }
    }

    internal class SqlIndexOutput : IndexOutput
    {
        /// <summary>
        /// Gets the current checksum of bytes written so far.
        /// </summary>
        public override long Checksum { get; }

        public SqlIndexOutput()
        {
            Checksum = 0;
        }

        public override void WriteByte(byte b)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
            throw new System.NotImplementedException();
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override long GetFilePointer()
        {
            throw new System.NotImplementedException();
        }

        [Obsolete("(4.1) this method will be removed in Lucene 5.0")]
        public override void Seek(long pos)
        {
            throw new System.NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new System.NotImplementedException();
        }
    }
}