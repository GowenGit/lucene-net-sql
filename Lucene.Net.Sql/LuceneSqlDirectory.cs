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
    public sealed class LuceneSqlDirectory : BaseDirectory
    {
        private readonly SqlDirectoryOptions _options;

        private readonly IDatabaseLuceneOperator _operator;

        public LuceneSqlDirectory(
            SqlDirectoryOptions options,
            IDatabaseLuceneOperator sqlOperator)
        {
            _options = options;
            _operator = sqlOperator;

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
            var length = _operator.GetNode(name)?.Size ?? 0;

            return length;
        }

        /// <inheritdoc/>
        public override IndexOutput CreateOutput(string name, IOContext context)
        {
            var node = _operator.GetNode(name);

            return new SqlIndexOutput(_options, _operator, node);
        }

        /// <inheritdoc/>
        public override IndexInput OpenInput(string name, IOContext context)
        {
            var node = _operator.GetNode(name);

            return new SqlIndexInput(_options, _operator, node, context);
        }

        /// <inheritdoc/>
        public override IndexInputSlicer CreateSlicer(string name, IOContext context)
        {
            var node = _operator.GetNode(name);

            return new SqlIndexInputSlicer(_options, _operator, node, context);
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
