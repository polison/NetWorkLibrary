﻿using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
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
            string connStr = $"data source={host};database={database};user id={user};password={password};pooling=true;charset=utf8;SslMode=None;";
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

        public bool Execute(string procedureName, string[] paramNames, object[] paramValues)
        {
            if (paramNames.Length != paramValues.Length)
            {
                LogManager.Instance.Log(LogType.Warning, "Execute {0} Error: param is not equal.", procedureName);
                return false;
            }

            MySqlCommand cmd = new MySqlCommand(procedureName, connection);
            for (int i = 0; i < paramNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            }

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
                LogManager.Instance.Log(LogType.Error, "Execute {0} Error: {1}.", procedureName, e.Message);
            }

            return false;
        }

        public MysqlResult Query(string procedureName, string[] paramNames, object[] paramValues)
        {
            MysqlResult result = new MysqlResult();
            if (paramNames.Length != paramValues.Length)
            {
                LogManager.Instance.Log(LogType.Warning, "Query {0} Error: param is not equal.", procedureName);
                return result;
            }

            MySqlCommand cmd = new MySqlCommand(procedureName, connection);
            for (int i = 0; i < paramNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            }

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
                LogManager.Instance.Log(LogType.Error, "Query {0} Error: {1}.", procedureName, e.Message);
            }

            return result;
        }

        public async void QueryAysnc(Action<MysqlResult> CallBack, string sql, string[] paramNames, object[] paramValues)
        {
            if (paramNames.Length != paramValues.Length)
            {
                LogManager.Instance.Log(LogType.Warning, "QueryAysnc {0} Error: param is not equal.", sql);
                return;
            }

            while (isQuerying)
            {
                await Task.Delay(10);
            }
            isQuerying = true;

            MysqlResult result = new MysqlResult();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            for (int i = 0; i < paramNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            }

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
                LogManager.Instance.Log(LogType.Error, "QueryAysnc {0} Error: {1}.", sql, e.Message);
            }
        }

        #region 存储过程查询

        public bool PExecute(string procedureName, string[] paramNames, object[] paramValues)
        {
            if (paramNames.Length != paramValues.Length)
            {
                LogManager.Instance.Log(LogType.Warning, "PExecute {0} Error: param is not equal.", procedureName);
                return false;
            }

            MySqlCommand cmd = new MySqlCommand(procedureName, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < paramNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            }

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
                LogManager.Instance.Log(LogType.Error, "PExecute {0} Error: {1}.", procedureName, e.Message);
            }

            return false;
        }

        public MysqlResult PQuery(string procedureName, string[] paramNames, object[] paramValues)
        {
            MysqlResult result = new MysqlResult();
            if (paramNames.Length != paramValues.Length)
            {
                LogManager.Instance.Log(LogType.Warning, "PQuery {0} Error: param is not equal.", procedureName);
                return result;
            }

            MySqlCommand cmd = new MySqlCommand(procedureName, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < paramNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            }

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
                LogManager.Instance.Log(LogType.Error, "PQuery {0} Error: {1}.", procedureName, e.Message);
            }

            return result;
        }

        public async void PQueryAysnc(Action<MysqlResult> CallBack, string procedureName, string[] paramNames, object[] paramValues)
        {
            if (paramNames.Length != paramValues.Length)
            {
                LogManager.Instance.Log(LogType.Warning, "PQueryAysnc {0} Error: param is not equal.", procedureName);
                return;
            }

            while (isQuerying)
            {
                await Task.Delay(10);
            }
            isQuerying = true;

            MysqlResult result = new MysqlResult();
            MySqlCommand cmd = new MySqlCommand(procedureName, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < paramNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            }

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
                LogManager.Instance.Log(LogType.Error, "PQueryAysnc {0} Error: {1}.", procedureName, e.Message);
            }
        }
        #endregion //存储过程查询

        public void Dispose()
        {
            isQuerying = false;
            connection.Dispose();
            asyncConnection.Dispose();
        }
    }
}
