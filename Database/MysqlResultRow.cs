using System;
using System.Data;

namespace NetWorkLibrary.Database
{
    public sealed class MysqlDataRow
    {
        private DataRow DataRow;

        public MysqlDataRow(DataRow Row)
        {
            DataRow = Row;
        }

        public T Read<T>(string columnName)
        {
            if (DataRow.IsNull(columnName))
                return default(T);
            return (T)Convert.ChangeType(DataRow[columnName], typeof(T));
        }

        public T Read<T>(int column)
        {
            if (DataRow.IsNull(column))
                return default(T);
            return (T)Convert.ChangeType(DataRow[column], typeof(T));
        }
    }
}
