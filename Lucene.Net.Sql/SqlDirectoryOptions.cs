namespace Lucene.Net.Sql
{
    public class SqlDirectoryOptions
    {
        /// <summary>
        /// Gets or sets SQL block size in bytes.
        /// </summary>
        public int BlockSize { get; set; } = 8192;

        /// <summary>
        /// Gets or sets SQL engine type.
        /// </summary>
        public SqlDirectoryEngine SqlDirectoryEngine { get; set; } = SqlDirectoryEngine.MySql;

        /// <summary>
        /// Gets or sets database connection string.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets directory file name.
        /// </summary>
        public string DirectoryName { get; set; } = string.Empty;
    }

    public enum SqlDirectoryEngine
    {
        MySql
    }
}