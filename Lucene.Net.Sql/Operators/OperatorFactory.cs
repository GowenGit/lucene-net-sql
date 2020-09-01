using System;

namespace Lucene.Net.Sql.Operators
{
    internal class OperatorFactory
    {
        public static IOperator Create(SqlDirectoryOptions options)
        {
            switch (options.SqlDirectoryEngine)
            {
                case SqlDirectoryEngine.MySql:
                    return new MySqlOperator(options);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}