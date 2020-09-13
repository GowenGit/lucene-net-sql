using System;
using J2N.Collections.Generic;
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

        void AddNode(string name);

        Node? GetNode(string name);

        void RemoveNode(string name);

        string AddLock(string lockName, string lockId);

        bool LockExists(string lockName);

        void RemoveLock(string lockName);

        byte[] GetBlock(long nodeId, long block);

        void WriteBlock(long nodeId, long block, byte[] data, long length);
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
            var sql = GetCommand(MySqlCommands.ListNodesQuery);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("directory", _options.DirectoryName);

            using var reader = command.ExecuteReader();

            var result = new List<string>();

            while (reader.Read())
            {
                result.Add(reader.GetString("name"));
            }

            return result.ToArray();
        }

        public void AddNode(string name)
        {
            var sql = GetCommand(MySqlCommands.CreateNodeCommand);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("directory", _options.DirectoryName);

            command.ExecuteNonQuery();
        }

        public Node? GetNode(string name)
        {
            var sql = GetCommand(MySqlCommands.GetNodeQuery);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("name", name);

            using var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                return null;
            }

            reader.Read();

            return new Node
            {
                Id = reader.GetInt64("id"),
                Name = reader.GetString("name"),
                Size = reader.GetInt64("size")
            };
        }

        public void RemoveNode(string name)
        {
            var sql = GetCommand(MySqlCommands.RemoveNodeCommand);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("name", name);

            command.ExecuteNonQuery();
        }

        public string AddLock(string lockName, string lockId)
        {
            var sql = GetCommand(MySqlCommands.AddLockCommand);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("anchor", lockName);
            command.Parameters.AddWithValue("lock_id", lockId);
            command.Parameters.AddWithValue("max_lock_time_in_seconds", 3600);

            var lockObjectId = command.ExecuteScalar() as string;

            return lockObjectId ?? string.Empty;
        }

        public bool LockExists(string lockName)
        {
            var sql = GetCommand(MySqlCommands.LockExistsQuery);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("anchor", lockName);

            var lockExists = command.ExecuteScalar() as int?;

            return lockExists == 1;
        }

        public void RemoveLock(string lockName)
        {
            var sql = GetCommand(MySqlCommands.DeleteLockCommand);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("anchor", lockName);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// TODO: optimize byte copy.
        /// </summary>
        public byte[] GetBlock(long nodeId, long block)
        {
            var sql = GetCommand(MySqlCommands.GetBlockCommand);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("node_id", nodeId);
            command.Parameters.AddWithValue("block", block);

            using var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                return Array.Empty<byte>();
            }

            reader.Read();

            var buffer = new byte[_options.BlockSize];

            reader.GetBytes(0, 0, buffer, 0, _options.BlockSize);

            return buffer;
        }

        public void WriteBlock(long nodeId, long block, byte[] data, long length)
        {
            var sql = GetCommand(MySqlCommands.WriteBlockCommand);

            using var connection = GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("node_id", nodeId);
            command.Parameters.AddWithValue("block", block);
            command.Parameters.AddWithValue("data", data);
            command.Parameters.AddWithValue("size", length);

            command.ExecuteNonQuery();
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
