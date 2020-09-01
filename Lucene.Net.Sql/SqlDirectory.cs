using System;
using System.Collections.Generic;
using Lucene.Net.Store;

namespace Lucene.Net.Sql
{
    /// <summary>
    /// An abstraction layer
    /// to store Lucene index
    /// files inside SQL Database.
    /// </summary>
    public sealed class SqlDirectory : BaseDirectory
    {
        public SqlDirectory(SqlDirectoryOptions options)
        {
            var lockFactory = new SqlLockFactory();

            SetLockFactory(lockFactory);
        }

        /// <inheritdoc/>
        public override string[] ListAll()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        [Obsolete("this method will be removed in 5.0")]
        public override bool FileExists(string name)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override void DeleteFile(string name)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override long FileLength(string name)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override IndexOutput CreateOutput(string name, IOContext context)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Sync(ICollection<string> names)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override IndexInput OpenInput(string name, IOContext context)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            throw new System.NotImplementedException();
        }
    }
}
