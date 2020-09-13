using System;

namespace Lucene.Net.Sql.Exceptions
{
    public sealed class LuceneSqlException : Exception
    {
        public LuceneSqlException(string message) : base(message) { }

        public LuceneSqlException(string message, Exception ex) : base(message, ex) { }
    }
}