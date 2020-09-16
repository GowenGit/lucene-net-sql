using System;
using J2N.Collections.Generic;
using Lucene.Net.Sql.Exceptions;
using Lucene.Net.Sql.Models;
using MySql.Data.MySqlClient;

#pragma warning disable CA2100

namespace Lucene.Net.Sql.MySql
{
    /// <summary>
    /// MySql adapter for
    /// IDatabaseLuceneOperator.
    /// </summary>
    public sealed class MySqlLuceneOperator : IDatabaseLuceneOperator
    {
        private readonly SqlDirectoryOptions _options;
        private readonly MySqlConnection _connection;

        private MySqlLuceneOperator(SqlDirectoryOptions options, MySqlConnection connection)
        {
            _options = options;
            _connection = connection;
        }

        public static MySqlLuceneOperator Create(SqlDirectoryOptions options)
        {
            var connection = new MySqlConnection(options.ConnectionString);

            try
            {
                connection.Open();

                var sqlOperator = new MySqlLuceneOperator(options, connection);

                sqlOperator.SetupTables();

                return sqlOperator;
            }
            catch (Exception ex)
            {
                connection.Dispose();

                throw new LuceneSqlException("Failed to create MySql Operator", ex);
            }
        }

        private void SetupTables()
        {
            var sql = GetCommand(MySqlCommands.SetupTablesCommand);

            using var command = new MySqlCommand(sql, _connection);

            command.ExecuteNonQuery();
        }

        internal void PurgeTables()
        {
            var sql = GetCommand(MySqlCommands.PurgeTablesCommand);

            using var command = new MySqlCommand(sql, _connection);

            command.ExecuteNonQuery();
        }

        public string[] ListNodes()
        {
            var sql = GetCommand(MySqlCommands.ListNodesQuery);

            using var command = new MySqlCommand(sql, _connection);

            command.Parameters.AddWithValue("directory", _options.DirectoryName);

            using var reader = command.ExecuteReader();

            var result = new List<string>();

            while (reader.Read())
            {
                result.Add(reader.GetString("name"));
            }

            return result.ToArray();
        }

        public Node GetNode(string name)
        {
            var sql = GetCommand(MySqlCommands.CreateIfNotExistsAndGetNodeQuery);

            using var command = new MySqlCommand(sql, _connection);

            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("directory", _options.DirectoryName);

            using var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                throw new LuceneSqlException("Node was expected");
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

            using var command = new MySqlCommand(sql, _connection);

            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("directory", _options.DirectoryName);

            command.ExecuteNonQuery();
        }

        public string AddLock(string lockName, string lockId)
        {
            var sql = GetCommand(MySqlCommands.AddLockCommand);

            using var command = new MySqlCommand(sql, _connection);

            command.Parameters.AddWithValue("anchor", lockName);
            command.Parameters.AddWithValue("lock_id", lockId);
            command.Parameters.AddWithValue("max_lock_time_in_seconds", 36000);

            var storedLockId = command.ExecuteScalar() as string;

            return storedLockId ?? string.Empty;
        }

        public bool LockExists(string lockName)
        {
            var sql = GetCommand(MySqlCommands.LockExistsQuery);

            using var command = new MySqlCommand(sql, _connection);

            command.Parameters.AddWithValue("anchor", lockName);

            return command.ExecuteScalar() as int? == 1;
        }

        public void RemoveLock(string lockName)
        {
            var sql = GetCommand(MySqlCommands.DeleteLockCommand);

            using var command = new MySqlCommand(sql, _connection);

            command.Parameters.AddWithValue("anchor", lockName);

            command.ExecuteNonQuery();
        }

        public void GetBlock(long nodeId, long block, byte[] destination, int srcOffset, int dstOffset, int length)
        {
            var sql = GetCommand(MySqlCommands.GetBlockCommand);

            using var command = new MySqlCommand(sql, _connection);

            command.Parameters.AddWithValue("node_id", nodeId);
            command.Parameters.AddWithValue("block", block);

            using var reader = command.ExecuteReader();

            if (!reader.HasRows)
            {
                return;
            }

            reader.Read();

            reader.GetBytes(0, srcOffset, destination, dstOffset, length);
        }

        public void WriteBlock(long nodeId, long block, byte[] data, long nodeLength)
        {
            var sql = GetCommand(MySqlCommands.WriteBlockCommand);

            using var command = new MySqlCommand(sql, _connection);

            command.Parameters.AddWithValue("node_id", nodeId);
            command.Parameters.AddWithValue("block", block);
            command.Parameters.AddWithValue("data", data);
            command.Parameters.AddWithValue("size", nodeLength);

            command.ExecuteNonQuery();
        }

        private string GetCommand(string text)
        {
            return string.Format(text, _options.TablePrefix);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}