using System;
using Lucene.Net.Sql.Models;

namespace Lucene.Net.Sql.Operators
{
    internal interface IOperator : IDisposable
    {
        void Initialise();

        void Purge();

        string[] ListNodes();

        Node? GetNode(string name);

        void RemoveNode(string name);

        string AddLock(string lockName, string lockId);

        bool LockExists(string lockName);

        void RemoveLock(string lockName);
    }

    internal class MySqlOperator : IOperator
    {
        private readonly SqlDirectoryOptions _options;

        public MySqlOperator(SqlDirectoryOptions options)
        {
            _options = options;
        }

        public void Initialise()
        {
            throw new NotImplementedException();
        }

        public void Purge()
        {
            throw new NotImplementedException();
        }

        public string[] ListNodes()
        {
            throw new NotImplementedException();
        }

        public Node? GetNode(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveNode(string name)
        {
            throw new NotImplementedException();
        }

        public string AddLock(string lockName, string lockId)
        {
            throw new NotImplementedException();
        }

        public bool LockExists(string lockId)
        {
            throw new NotImplementedException();
        }

        public void RemoveLock(string lockName)
        {
            throw new NotImplementedException();
        }

        public void Dispose() { }
    }
}
