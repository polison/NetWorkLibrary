using System;
using System.Data;

namespace NetWorkLibrary.Database
{
    public sealed class MysqlResult : DataTable
    {
        public int RowCount { get; set; }

        public int ColumnCount { get; set; }

        public T Read<T>(int row, string columnName)
        {
            return (T)Convert.ChangeType(Rows[row][columnName], typeof(T));
        }

        public T Read<T>(int row, int column)
        {
            return (T)Convert.ChangeType(Rows[row][column], typeof(T));
        }
    }
}
