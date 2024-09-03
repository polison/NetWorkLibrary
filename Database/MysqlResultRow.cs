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

        public bool IsNull(string columnName)
        {
            return DataRow.IsNull(columnName);
        }

        public bool IsNull(int column)
        {
            return DataRow.IsNull(column);
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
