using System;
using System.Data.SqlClient;
using J2N.Collections.Generic;
using Lucene.Net.Sql.Exceptions;
using Lucene.Net.Sql.Models;

#pragma warning disable CA2100

namespace Lucene.Net.Sql.SqlServer
{
    /// <summary>
    /// SQL Server adapter for
    /// IDatabaseLuceneOperator.
    /// </summary>
    public sealed class SqlServerLuceneOperator : IDatabaseLuceneOperator
    {
        private readonly SqlDirectoryOptions _options;
        private readonly SqlConnection _connection;

        private SqlServerLuceneOperator(SqlDirectoryOptions options, SqlConnection connection)
        {
            _options = options;
            _connection = connection;
        }

        public static SqlServerLuceneOperator Create(SqlDirectoryOptions options)
        {
            var connection = new SqlConnection(options.ConnectionString);

            try
            {
                connection.Open();

                var sqlOperator = new SqlServerLuceneOperator(options, connection);

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
            var sql = GetCommand(SqlServerCommands.SetupTablesCommand);

            using var command = new SqlCommand(sql, _connection);

            command.ExecuteNonQuery();
        }

        internal void PurgeTables()
        {
            var sql = GetCommand(SqlServerCommands.PurgeTablesCommand);

            using var command = new SqlCommand(sql, _connection);

            command.ExecuteNonQuery();
        }

        public string[] ListNodes()
        {
            var sql = GetCommand(SqlServerCommands.ListNodesQuery);

            using var command = new SqlCommand(sql, _connection);

            command.Parameters.AddWithValue("directory", _options.DirectoryName);

            using var reader = command.ExecuteReader();

            var result = new List<string>();

            while (reader.Read())
            {
                result.Add(reader.GetString(reader.GetOrdinal("name")));
            }

            return result.ToArray();
        }

        public Node GetNode(string name)
        {
            var sql = GetCommand(SqlServerCommands.CreateIfNotExistsAndGetNodeQuery);

            using var command = new SqlCommand(sql, _connection);

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
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Size = reader.GetInt64(reader.GetOrdinal("size"))
            };
        }

        public void RemoveNode(string name)
        {
            var sql = GetCommand(SqlServerCommands.RemoveNodeCommand);

            using var command = new SqlCommand(sql, _connection);

            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("directory", _options.DirectoryName);

            command.ExecuteNonQuery();
        }

        public string AddLock(string lockName, string lockId)
        {
            var sql = GetCommand(SqlServerCommands.AddLockCommand);

            using var command = new SqlCommand(sql, _connection);

            command.Parameters.AddWithValue("anchor", lockName);
            command.Parameters.AddWithValue("lock_id", lockId);
            command.Parameters.AddWithValue("max_lock_time_in_seconds", 36000);

            var storedLockId = command.ExecuteScalar() as string;

            return storedLockId ?? string.Empty;
        }

        public bool LockExists(string lockName)
        {
            var sql = GetCommand(SqlServerCommands.LockExistsQuery);

            using var command = new SqlCommand(sql, _connection);

            command.Parameters.AddWithValue("anchor", lockName);

            return command.ExecuteScalar() as int? == 1;
        }

        public void RemoveLock(string lockName)
        {
            var sql = GetCommand(SqlServerCommands.DeleteLockCommand);

            using var command = new SqlCommand(sql, _connection);

            command.Parameters.AddWithValue("anchor", lockName);

            command.ExecuteNonQuery();
        }

        public void GetBlock(long nodeId, long block, byte[] destination, int srcOffset, int dstOffset, int length)
        {
            var sql = GetCommand(SqlServerCommands.GetBlockCommand);

            using var command = new SqlCommand(sql, _connection);

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
            var sql = GetCommand(SqlServerCommands.WriteBlockCommand);

            using var command = new SqlCommand(sql, _connection);

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