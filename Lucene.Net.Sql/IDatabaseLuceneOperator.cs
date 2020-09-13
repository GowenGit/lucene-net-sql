using System;
using Lucene.Net.Sql.Models;

namespace Lucene.Net.Sql
{
    public interface IDatabaseLuceneOperator : IDisposable
    {
        string[] ListNodes();

        Node GetNode(string name);

        void RemoveNode(string name);

        string AddLock(string lockName, string lockId);

        bool LockExists(string lockName);

        void RemoveLock(string lockName);

        void GetBlock(long nodeId, long block, byte[] destination, int srcOffset, int dstOffset, int length);

        void WriteBlock(long nodeId, long block, byte[] data, long nodeLength);
    }
}
