using System;
using Lucene.Net.Sql.Models;
using Lucene.Net.Sql.Schema;
using MySql.Data.MySqlClient;

#pragma warning disable CA2100

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
            var sql = GetCommand(MySqlCommands.InitializeCommand);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.ExecuteNonQuery();
        }

        public void Purge()
        {
            var sql = GetCommand(MySqlCommands.PurgeCommand);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.ExecuteNonQuery();
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

        private MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(_options.ConnectionString);

            connection.Open();

            return connection;
        }

        private string GetCommand(string text)
        {
            return string.Format(text, _options.TablePrefix);
        }

        public void Dispose() { }
    }
}
