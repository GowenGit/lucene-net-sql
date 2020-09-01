using System;

namespace Lucene.Net.Sql.Exceptions
{
    public sealed class LuceneSqlLockException : Exception
    {
        public LuceneSqlLockException(string message) : base(message) { }

        public LuceneSqlLockException(string message, Exception ex) : base(message, ex) { }
    }
}
