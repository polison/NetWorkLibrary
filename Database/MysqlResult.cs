using System.Data;

namespace NetWorkLibrary.Database
{
    public sealed class MysqlResult : DataTable
    {
        public int RowCount { get; set; }

        public int ColumnCount { get; set; }

        public MysqlDataRow Read(int row)
        {
            return new MysqlDataRow(Rows[row]);
        }

        public T Read<T>(int row, string columnName)
        {
            MysqlDataRow datarow = Read(row);
            return datarow.Read<T>(columnName);
        }

        public T Read<T>(int row, int column)
        {
            MysqlDataRow datarow = Read(row);
            return datarow.Read<T>(column);
        }
    }
}
