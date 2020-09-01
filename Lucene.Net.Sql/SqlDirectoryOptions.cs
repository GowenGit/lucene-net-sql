using System;

namespace Lucene.Net.Sql
{
    public class SqlDirectoryOptions
    {
        /// <summary>
        /// Gets SQL block size in bytes.
        /// </summary>
        public int BlockSize { get; }

        /// <summary>
        /// Gets SQL engine type.
        /// </summary>
        public SqlDirectoryEngine SqlDirectoryEngine { get; }

        /// <summary>
        /// Gets database connection string.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets directory file name.
        /// </summary>
        public string DirectoryName { get; }

        /// <summary>
        /// Gets SQL table prefix.
        /// </summary>
        public string TablePrefix { get; }

        public SqlDirectoryOptions(
            SqlDirectoryEngine engineType,
            string connectionString,
            string directoryName,
            string tablePrefix = "lucene_fs",
            int blockSize = 8192)
        {
            SqlDirectoryEngine = engineType;

            ConnectionString = !string.IsNullOrWhiteSpace(connectionString)
                ? connectionString :
                  throw new ArgumentException("Argument can not be null or empty", nameof(connectionString));

            DirectoryName = !string.IsNullOrWhiteSpace(directoryName)
                ? directoryName :
                throw new ArgumentException("Argument can not be null or empty", nameof(directoryName));

            TablePrefix = !string.IsNullOrWhiteSpace(tablePrefix)
                ? tablePrefix :
                throw new ArgumentException("Argument can not be null or empty", nameof(tablePrefix));

            BlockSize = blockSize > 1023
                ? blockSize :
                throw new ArgumentException("Argument can not be less than 1024", nameof(blockSize));
        }
    }

    public enum SqlDirectoryEngine
    {
        MySql
    }
}