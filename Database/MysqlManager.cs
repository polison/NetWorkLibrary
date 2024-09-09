using MySql.Data.MySqlClient;
using NetWorkLibrary.Utility;
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkLibrary.Database
{
    public sealed class MysqlManager : IDisposable
    {
        bool isQuerying = false;
        MySqlConnection connection;
        MySqlConnection asyncConnection;
        public void Init(string host, string user, string password, string database, int port)
        {
            string connStr = $"data source={host};database={database};user id={user};password={password};pooling=true;charset=utf8;";
            connection = new MySqlConnection(connStr);
            asyncConnection = new MySqlConnection(connStr);

            try
            {
                connection.Open();
                asyncConnection.Open();
                LogManager.Instance.Log(LogType.Message, "Mysql Successfully connected to {0}:{1}:{2}", host, port, database);
            }
            catch (MySqlException e)
            {
                LogManager.Instance.Log(LogType.Error, "{0}", e.Message);
            }
        }

        public bool Execute(string sql, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.GetCultureInfo("en-US").NumberFormat, sql, args);
            var slqStr = sb.ToString();
            MySqlCommand cmd = new MySqlCommand(slqStr, connection);

            try
            {
                lock (connection)
                {
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (MySqlException e)
            {
                LogManager.Instance.Log(LogType.Error, "Execute {0} Error: {1}.", slqStr, e.Message);
            }
            return false;
        }

        public MysqlResult Query(string sql, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.GetCultureInfo("en-US").NumberFormat, sql, args);

            MysqlResult result = new MysqlResult();
            var slqStr = sb.ToString();
            MySqlCommand cmd = new MySqlCommand(slqStr, connection);

            try
            {
                lock (connection)
                {
                    var reader = cmd.ExecuteReader(CommandBehavior.Default);
                    result.Load(reader);
                    reader.Close();
                }
                result.RowCount = result.Rows.Count;
                result.ColumnCount = result.Columns.Count;
            }
            catch (MySqlException e)
            {
                LogManager.Instance.Log(LogType.Error, "Select {0} Error: {1}.", slqStr, e.Message);
            }

            return result;
        }

        public async void QueryAysnc(Action<MysqlResult> CallBack, string sql, params object[] args)
        {
            while (isQuerying)
            {
                await Task.Delay(10);
            }
            isQuerying = true;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.GetCultureInfo("en-US").NumberFormat, sql, args);

            MysqlResult result = new MysqlResult();
            var slqStr = sb.ToString();
            MySqlCommand cmd = new MySqlCommand(slqStr, asyncConnection);

            try
            {
                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.Default);
                result.Load(reader);
                reader.Close();
                isQuerying = false;
                result.RowCount = result.Rows.Count;
                result.ColumnCount = result.Columns.Count;
                CallBack(result);
            }
            catch (MySqlException e)
            {
                LogManager.Instance.Log(LogType.Error, "Async Select {0} Error: {1}.", slqStr, e.Message);
            }
        }

        public void Dispose()
        {
            isQuerying = false;
            connection.Dispose();
            asyncConnection.Dispose();
        }
    }
}
