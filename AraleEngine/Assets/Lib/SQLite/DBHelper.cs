/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Text;
using System;
using System.IO;

[Serializable]
public class DBHelper
{
    public string dbPath;
    private SqliteConnection conn;

    private static DBHelper instance;

    public static DBHelper Instance
    {
        get
        {
            if (instance == null)
                instance = new DBHelper();
            return instance;
        }
    }

    public void OpenConnection()
    {
        if (!string.IsNullOrEmpty(dbPath))
        {
            var connStr = new SqliteConnectionStringBuilder();
            connStr.DataSource = dbPath;

            conn = new SqliteConnection(connStr.ToString());
            conn.Open();
        }
    }

    public void Close()
    {
        if (conn != null)
        {
            conn.Close();
            conn = null;
        }
    }

    public int ExecuteNonQuery(string sql, SqliteParameter[] parameter = null)
    {
        var cmd = CreateCommand(sql, parameter);
        return cmd.ExecuteNonQuery();
    }

    public object ExecuteScalar(string sql, SqliteParameter[] parameter = null)
    {
        var cmd = CreateCommand(sql, parameter);
        return cmd.ExecuteScalar();
    }

    public void ExecuteReader(string sql, Action<SqliteDataReader> callback, params SqliteParameter[] param)
    {
        if (callback != null)
        {
            var cmd = CreateCommand(sql, param);
            var reader = cmd.ExecuteReader();
            callback(reader);
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
        }
    }

    public List<Dictionary<string, object>> QueryTable(string tableName, params string[] colNames)
    {
        if (colNames.Length > 0)
        {
            var sb = new StringBuilder();
            sb.Append("select ");
            foreach (var colName in colNames)
            {
                sb.AppendFormat("{0},", colName);
            }
            sb.Remove(sb.Length - 1, 1);
            sb.AppendFormat(" from {0}", tableName);

            var list = new List<Dictionary<string, object>>();
            ExecuteReader(sb.ToString(), (reader) => {
                while (reader.Read())
                {
                    var dict = new Dictionary<string, object>();
                    list.Add(dict);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dict[colNames[i]] = reader[i];
                    }
                }
            }, null);
            return list;
        }
        return null;
    }
    private void CheckConn()
    {
        if (conn == null)
            OpenConnection();
    }

    private SqliteCommand CreateCommand(string sql, SqliteParameter[] parameter = null)
    {
        if (!string.IsNullOrEmpty(sql))
        {
            CheckConn();
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (parameter != null)
                cmd.Parameters.AddRange(parameter);
            return cmd;
        }
        else
        {
            throw new Exception("sql is null");
        }
    }

    /// <summary>
    /// 一次读出 blob数据.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public static byte[] GetBytes(SqliteDataReader reader, int col)
    {
        const int SIZE = 128;
        byte[] buffer = new byte[SIZE];
        long bytesRead;
        long fieldOffset = 0;
        using (MemoryStream ms = new MemoryStream())
        {
            while ((bytesRead = reader.GetBytes(col, fieldOffset, buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, (int)bytesRead);
                fieldOffset += bytesRead;
            }
            return ms.ToArray();
        }
    }

    public static IEnumerator WaitForGetBytesFromStreamingAssets(string fileName, Action<byte[]> onOk)
    {
        var path = Path.Combine(Application.streamingAssetsPath, fileName);
        byte[] result;
        if (path.Contains("://"))
        {
            var www = new WWW(path);
            yield return www;
            result = www.bytes;
            www.Dispose();
        }
        else
        {
            result = File.ReadAllBytes(path);
        }
        if (onOk != null)
        {
            onOk(result);
            onOk = null;
        }
    }

    public static void MoveToTempPath(string fileName, byte[] bytes)
    {
        var path = Path.Combine(Application.temporaryCachePath, fileName);
        File.WriteAllBytes(path, bytes);
    }

    public static IEnumerator WaitForMoveToTempFromStreamingAssets(string fileNameInStreamingAsset, Action callback = null)
    {
        yield return WaitForGetBytesFromStreamingAssets(fileNameInStreamingAsset, bytes => MoveToTempPath(fileNameInStreamingAsset, bytes));
        if (callback != null)
        {
            callback();
            callback = null;
        }
    }
}*/