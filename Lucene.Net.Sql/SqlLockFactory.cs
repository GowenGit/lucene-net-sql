using System;
using Lucene.Net.Sql.Exceptions;
using Lucene.Net.Sql.Operators;
using Lucene.Net.Store;

namespace Lucene.Net.Sql
{
    internal class SqlLockFactory : LockFactory
    {
        private readonly IOperator _sqlOperator;
        private readonly SqlDirectoryOptions _options;

        public SqlLockFactory(IOperator sqlOperator, SqlDirectoryOptions options)
        {
            _sqlOperator = sqlOperator;
            _options = options;
        }

        public override Lock MakeLock(string lockName)
        {
            lockName = CreateFullLockName(lockName);

            return new SqlLock(_sqlOperator, lockName);
        }

        public override void ClearLock(string lockName)
        {
            lockName = CreateFullLockName(lockName);

            try
            {
                _sqlOperator.RemoveLock(lockName);
            }
            catch (Exception ex)
            {
                throw new LuceneSqlLockException($"Cannot remove lock entry {lockName}", ex);
            }
        }

        private string CreateFullLockName(string lockName)
        {
            return $"{_options.DirectoryName}-{lockName}";
        }
    }

    internal class SqlLock : Lock
    {
        private readonly IOperator _sqlOperator;
        private readonly string _lockName;
        private readonly string _lockId;

        public SqlLock(IOperator sqlOperator, string lockName)
        {
            _sqlOperator = sqlOperator;
            _lockName = lockName;
            _lockId = CreateLockId();
        }

        public override bool Obtain()
        {
            var lockId = _sqlOperator.AddLock(_lockName, _lockId);

            if (lockId == _lockId)
            {
                return true;
            }

            FailureReason = new LuceneSqlLockException($"Failed to obtain lock for lockId: {_lockId}, lock for {_lockName} already exists and was issued by {lockId}");
            return false;
        }

        public override bool IsLocked()
        {
            return _sqlOperator.LockExists(_lockId);
        }

        private static string CreateLockId()
        {
            return Guid.NewGuid().ToString().ToLower();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            try
            {
                _sqlOperator.RemoveLock(_lockName);
            }
            catch (Exception ex)
            {
                throw new LuceneSqlLockException($"Cannot remove lock entry {_lockName}", ex);
            }
        }

        public override string ToString()
        {
            return $"sql-lock-{_lockName}-{_lockId}";
        }
    }
}