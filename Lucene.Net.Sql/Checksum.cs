namespace Lucene.Net.Sql
{
    internal interface IChecksum
    {
        void Reset();

        void Update(int val);

        void Update(byte[] b);

        void Update(byte[] b, int offset, int length);

        long Value { get; }
    }

    /// <summary>
    /// CRC32 checksum.
    /// </summary>
    internal class Checksum : IChecksum
    {
        private static readonly uint[] CrcTable = InitializeTable();

        /// <summary>
        /// Build polynomial lookup table.
        /// </summary>
        private static uint[] InitializeTable()
        {
            var crcTable = new uint[256];

            for (uint n = 0; n < 256; n++)
            {
                var c = n;

                for (var k = 8; --k >= 0;)
                {
                    if ((c & 1) != 0)
                    {
                        c = 0xedb88320 ^ (c >> 1);
                    }
                    else
                    {
                        c >>= 1;
                    }
                }

                crcTable[n] = c;
            }

            return crcTable;
        }

        private uint _crc;

        public long Value => _crc & 0xffffffffL;

        public void Reset()
        {
            _crc = 0;
        }

        public void Update(int val)
        {
            // reverse each bit
            var c = ~_crc;

            c = CrcTable[(c ^ val) & 0xff] ^ (c >> 8);

            _crc = ~c;
        }

        public void Update(byte[] buf, int off, int len)
        {
            // reverse each bit
            var c = ~_crc;

            while (--len >= 0)
            {
                c = CrcTable[(c ^ buf[off++]) & 0xff] ^ (c >> 8);
            }

            _crc = ~c;
        }

        public void Update(byte[] buf)
        {
            Update(buf, 0, buf.Length);
        }
    }
}