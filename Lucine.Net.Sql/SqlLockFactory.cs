using System;
using Lucene.Net.Store;

namespace Lucine.Net.Sql
{
    internal class SqlLockFactory : LockFactory
    {
        public override Lock MakeLock(string lockName)
        {
            throw new NotImplementedException();
        }

        public override void ClearLock(string lockName)
        {
            throw new NotImplementedException();
        }
    }

    internal class SqlLock : Lock
    {
        public override bool Obtain()
        {
            throw new NotImplementedException();
        }

        public override bool IsLocked()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }
    }
}