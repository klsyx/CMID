using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace CMID
{
    class DatabaseHelper
    {
        private readonly string connectionString;

        public DatabaseHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool HasResults(List<Dictionary<string, object>> results)
        {
            return results != null && results.Count > 0;
        }

        // 查询方法
        public List<Dictionary<string, object>> Query(string tableName, string whereClause = "", object fields = null)
        {
            var outputFields = ConvertToList<string>(fields);
            var sql = BuildSelectQuery(tableName, whereClause, outputFields);

            return ExecuteQuery(sql, null, cmd =>
            {
                using (var reader = cmd.ExecuteReader())
                {
                    var results = ReadResults(reader);
                    return results;  // 无论结果数量多少，都统一返回 List<Dictionary<string, object>>
                }
            });
        }

        // 插入方法
        public int Insert(string tableName, object fields, object values, string condition = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(condition))
                {
                    var existingRecord = Query(tableName, condition, fields);
                    if (existingRecord.Count > 0)
                    {
                        return 0; // 条件已满足，不执行插入
                    }
                }

                var fieldsList = ConvertToList<string>(fields);
                var valuesList = ConvertToList<object>(values);

                var parameters = CreateParameters(fieldsList, valuesList);
                var sql = $"INSERT INTO {tableName} ({string.Join(", ", fieldsList)}) VALUES ({string.Join(", ", fieldsList.Select(f => $"@{f}"))})";

                return ExecuteNonQuery(sql, parameters);
            }
            catch
            {
                return 0; // 插入失败时返回0
            }
        }

        // 更新方法
        public int Update(string tableName, string whereClause, object fields, object values, int checkBeforeUpdate = 0, string checkField = null, object checkValue = null, string updateField = null, object updateValue = null)
        {
            var fieldsList = ConvertToList<string>(fields);
            var valuesList = ConvertToList<object>(values);

            if (string.IsNullOrEmpty(checkField) && checkValue == null)
            {
                checkField = fieldsList.FirstOrDefault();
                checkValue = valuesList.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(updateField))
            {
                updateField = fieldsList.FirstOrDefault();
                updateValue = valuesList.FirstOrDefault();
            }

            if (checkBeforeUpdate == 1)
            {
                var existingRecord = Query(tableName, whereClause, checkField);
                if (existingRecord.Any(record => record[checkField]?.Equals(checkValue) == true))
                {
                    fieldsList.Clear();
                    valuesList.Clear();
                    fieldsList.Add(updateField);
                    valuesList.Add(updateValue);
                }
                else
                {
                    return 0;
                }
            }

            var parameters = CreateParameters(fieldsList, valuesList);
            var sql = $"UPDATE {tableName} SET {string.Join(", ", fieldsList.Select(f => $"{f} = @{f}"))} WHERE {whereClause}";

            return ExecuteNonQuery(sql, parameters);
        }

        // 删除方法
        public int Delete(string tableName, string whereClause)
        {
            var sql = $"DELETE FROM {tableName} WHERE {whereClause}";
            return ExecuteNonQuery(sql, null);
        }

        // 新增封装的方法：创建参数化的 SQL 查询参数
        private Dictionary<string, object> CreateParameters(List<string> fields, List<object> values)
        {
            return fields.Zip(values, (field, value) => new { field, value })
                         .ToDictionary(pair => pair.field, pair => pair.value);
        }

        // 新增封装的方法：执行参数化查询
        private dynamic ExecuteQuery(string sql, Dictionary<string, object> parameters, Func<SqlCommand, dynamic> execute)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        AddParameters(command, parameters);
                    }

                    return execute(command);
                }
            }
        }

        // 执行非查询操作
        private int ExecuteNonQuery(string sql, Dictionary<string, object> parameters)
        {
            return ExecuteQuery(sql, parameters, cmd => cmd.ExecuteNonQuery());
        }

        // 构建SELECT查询语句
        private string BuildSelectQuery(string tableName, string whereClause, List<string> outputFields)
        {
            var selectFields = outputFields == null || outputFields.Count == 0 ? "*" : string.Join(", ", outputFields);
            var sql = $"SELECT {selectFields} FROM {tableName}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += $" WHERE {whereClause}";
            }

            return sql;
        }

        // 读取查询结果
        private List<Dictionary<string, object>> ReadResults(IDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();

            while (reader.Read())
            {
                var result = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    result[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(result);
            }

            return results;
        }

        // 添加参数到 SQL 命令对象
        private void AddParameters(SqlCommand command, Dictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value ?? DBNull.Value);
            }
        }

        // 将输入转化为列表形式
        private List<T> ConvertToList<T>(object input)
        {
            if (input == null) return new List<T>();
            if (input is List<T> list) return list;
            return new List<T> { (T)input };
        }
    }
}
