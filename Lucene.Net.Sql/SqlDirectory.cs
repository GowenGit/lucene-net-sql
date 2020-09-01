using System;
using System.Collections.Generic;
using Lucene.Net.Sql.Operators;
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
        private readonly IOperator _operator;

        public SqlDirectory(SqlDirectoryOptions options)
        {
            _operator = OperatorFactory.Create(options);

            var lockFactory = new SqlLockFactory(_operator, options);

            SetLockFactory(lockFactory);
        }

        /// <inheritdoc/>
        public override string[] ListAll()
        {
            return _operator.ListNodes();
        }

        /// <inheritdoc/>
        [Obsolete("this method will be removed in 5.0")]
        public override bool FileExists(string name)
        {
            return _operator.GetNode(name) != null;
        }

        /// <inheritdoc/>
        public override void DeleteFile(string name)
        {
            _operator.RemoveNode(name);
        }

        /// <inheritdoc/>
        public override long FileLength(string name)
        {
            return _operator.GetNode(name)?.Size ?? 0;
        }

        /// <inheritdoc/>
        public override IndexOutput CreateOutput(string name, IOContext context)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override IndexInput OpenInput(string name, IOContext context)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        /// Not used since we sync during Flush operations.
        public override void Sync(ICollection<string> names) { }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _operator.Dispose();
            }
        }
    }
}
