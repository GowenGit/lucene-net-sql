using System;

#pragma warning disable CA1822

namespace Lucene.Net.Sql
{
    public class SqlDirectoryOptions
    {
        /// <summary>
        /// Gets SQL block size in bytes.
        /// This can not be changed once data
        /// starts to be written so not configurable
        /// at the moment. Can extend later if we add
        /// checks to verify that there are no written
        /// block of old size.
        /// </summary>
        public int BlockSize { get; internal set; } = 16384;

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
            string tablePrefix = "lucene_fs")
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
        }
    }

    public enum SqlDirectoryEngine
    {
        MySql
    }
}