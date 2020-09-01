using System;

namespace Lucene.Net.Sql.Operators
{
    internal class OperatorFactory
    {
        public static IOperator Create(SqlDirectoryOptions options)
        {
            IOperator sqlOperator;

            switch (options.SqlDirectoryEngine)
            {
                case SqlDirectoryEngine.MySql:
                    sqlOperator = new MySqlOperator(options);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            sqlOperator.Initialise();

            return sqlOperator;
        }
    }
}