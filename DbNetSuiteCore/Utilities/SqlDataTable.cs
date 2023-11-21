using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DbNetSuiteCore.Services;
using DbNetSuiteCore.ViewModels;
using Microsoft.Data.Sqlite;

namespace DbNetSuiteCore.Utilities
{
    public class SqlDataTableColumn
    {
        public string ColumnName { get; set; }
        public Type DataType { get; set; }
        public SqlDataTableColumn() { }
        public SqlDataTableColumn(string columnName, Type dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }
    }

    public class SqlDataTable : IDisposable
    {
        private readonly SqliteConnection _Connection;
        private readonly SqliteCommand _Command;
        private SqliteDataReader _Reader;
        private bool _TableCreated = false;

        List<SqlDataTableColumn> Columns { get; set; } = new List<SqlDataTableColumn>();

        public SqlDataTable()
        {
            _Connection = new SqliteConnection("Data Source=InMemory;Mode=Memory;Cache=Shared;");
            _Connection.Open();
            _Command = _Connection.CreateCommand();
        }

        public void Dispose()
        {
            _Connection.Close();
            _Connection.Dispose();
        }

        public void AddColumn(SqlDataTableColumn column)
        {
            Columns.Add(column);
        }

        public void AddColumn(string columnName, Type dataType)
        {
            AddColumn(new SqlDataTableColumn(columnName, dataType));
        }

        public async Task AddRow<T>(T record)
        {
            Dictionary<string,object> values = RecordToDictionary(record, Columns.Any() == false);
            if (_TableCreated == false)
            {
                await CreateTable();
            }
            ConfigureCommand($"insert into [FileInfo] ({string.Join(",", values.Keys)}) values ({string.Join(",", values.Keys.Select(c => $"@{c}").ToList())});", values);
            await _Command.ExecuteNonQueryAsync();
        }

        public async Task AddRow(Dictionary<string, object> values)
        {
            UpdateColumns(values);
            if (_TableCreated == false)
            {
                await CreateTable();
            }
            ConfigureCommand($"insert into [FileInfo] ({string.Join(",", values.Keys)}) values ({string.Join(",", values.Keys.Select(c => $"@{c}").ToList())});", values);
            await _Command.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> Query()
        {
            return await Query(null, null);
        }

        public async Task<DataTable> Query(string filter)
        {
            return await Query(filter, null);
        }

        public async Task<DataTable> Query(string filter = null, Dictionary<string, object> filterParameters = null)
        {
            string sql = $"select * from [FileInfo] where {(string.IsNullOrEmpty(filter) ? "1=1" : filter)}";
            ConfigureCommand(sql, filterParameters);
            _Reader = await _Command.ExecuteReaderAsync();
            var fieldCount = _Reader.FieldCount;
            DataTable dataTable = new DataTable();
            List<int> boolColumns = new List<int>();
            foreach (SqlDataTableColumn column in Columns)
            {
                if (column.DataType == typeof(bool))
                {
                    boolColumns.Add(dataTable.Columns.Count);
                }
                dataTable.Columns.Add(column.ColumnName, column.DataType);
            }
            while (await _Reader.ReadAsync())
            {
                object[] values = new object[fieldCount];
                _Reader.GetValues(values);
                foreach (int boolIndex in boolColumns)
                {
                    values[boolIndex] = Convert.ToBoolean(Convert.ToInt32(values[boolIndex]));
                }
                dataTable.Rows.Add(values);
            }
            dataTable.Load(_Reader);
            return dataTable;
        }

        private async Task CreateTable()
        {
            List<string> columns = new List<string>();

            foreach (SqlDataTableColumn column in Columns)
            {
                columns.Add($"{column.ColumnName} {SqlLiteType(column.DataType)}");
            }

            ConfigureCommand($"CREATE TABLE [FileInfo] ({string.Join(",", columns)});");
            await _Command.ExecuteNonQueryAsync();
            _TableCreated = true;
        }

        private string SqlLiteType(Type dataType)
        {
            string sqlType = "TEXT";
            switch (dataType.ToString())
            {
                case nameof(DateOnly):
                    sqlType = "DATE";
                    break;
                case nameof(DateTime):
                    sqlType = "DATETIME";
                    break;
                case nameof(UInt16):
                case nameof(UInt32):
                case nameof(UInt64):
                case nameof(UInt128):
                case nameof(Int16):
                case nameof(Int32):
                case nameof(Int64):
                case nameof(Int128):
                case nameof(SByte):
                case nameof(Boolean):
                case nameof(Byte):
                    sqlType = "INTEGER";
                    break;
                case nameof(Decimal):
                case nameof(Double):
                case nameof(Single):
                    sqlType = "REAL";
                    break;
            }

            return sqlType;
        }

        public void UpdateColumns(Dictionary<string, object> values)
        {
            foreach (string key in values.Keys)
            {
                object value = values[key];
                var column = Columns.FirstOrDefault(c => c.ColumnName.ToLower() == key.ToLower());
                if (column != null)
                {
                    if (value != null)
                    {
                        if (value.GetType() != column.DataType)
                        {
                            column.DataType = value.GetType();
                        }
                    }
                }
                else
                {
                    Columns.Add(new SqlDataTableColumn(key, value == null ? typeof(string) : value.GetType()));
                }
            }
        }

        private void ConfigureCommand(string sql, IDictionary parameters = null)
        {
            _Command.CommandText = sql.Trim();
            _Command.CommandType = CommandType.Text;
            _Command.Parameters.Clear();
            AddCommandParameters(parameters);
        }

        private void AddCommandParameters(IDictionary parameters)
        {
            if (parameters == null)
                return;

            foreach (string key in parameters.Keys)
            {
                SqliteParameter dbParam = _Command.CreateParameter();
                dbParam.ParameterName = key.StartsWith("@") ? key : $"@{key}";
                dbParam.Value = parameters[key] ?? DBNull.Value;
                _Command.Parameters.Add(dbParam);
            }
        }

        public Dictionary<string,object> RecordToDictionary(object record, bool updateColumns)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            Type type = record.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead || IsSimple(property.PropertyType) == false)
                {
                    continue;
                }

                if (updateColumns)
                {
                    AddColumn(property.Name, property.PropertyType);
                }
                dictionary[property.Name] = property.GetValue(record, null);
            }

            return dictionary;
        }

        private bool IsSimple(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }
    }
}
