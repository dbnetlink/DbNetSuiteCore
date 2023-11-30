using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        private SqliteCommand _InsertCommand;
        private bool _TableCreated = false;
        private string _TableName = "SqlData";
        public SqliteConnection Connection => _Connection;

        List<SqlDataTableColumn> Columns { get; set; } = new List<SqlDataTableColumn>();

        public SqlDataTable()
        {
            _Connection = new SqliteConnection("Data Source=:memory:;Mode=Memory;Cache=Private;");
            _Connection.Open();
            _InsertCommand = _Connection.CreateCommand();
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

        public async Task AddRows(DataTable dataTable)
        {
            if (dataTable.Rows.Count == 0)
            {
                throw new Exception("DataTable does not contain any rows");
            }
            UpdateColumns(DataRowToDictionary(dataTable.Rows[0]));
            await CreateTable(dataTable.TableName);
            using (SqliteTransaction transaction = _Connection.BeginTransaction())
            {
                _InsertCommand.Transaction = transaction;
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    await AddRow(dataRow);
                }

                await transaction.CommitAsync();
            }
        }
        public async Task AddRows<T>(List<T> records)
        {
            T record = records.First();
            UpdateColumns(RecordToDictionary(record));
            await CreateTable(record.GetType().Name);
            using (SqliteTransaction transaction = _Connection.BeginTransaction())
            {
                _InsertCommand.Transaction = transaction;
                foreach (T r in records)
                {
                    await AddRow(r);
                }

                await transaction.CommitAsync();
            }
        }

        public async Task AddRow<T>(T record)
        {
            if (IsGenericList(record))
            {
                throw new Exception("Use AddRows for a generic list");
            }
            Dictionary<string, object> values;

            if (typeof(T) == typeof(DataRow))
            {
                values = DataRowToDictionary(record as DataRow, Columns.Any() == false);
            }
            else
            {   
                values = RecordToDictionary(record, Columns.Any() == false);
            }
            await AddRow(values, typeof(T).Name);
        }

        public async Task AddRow(Dictionary<string, object> values, string tableName = "SqlData")
        {
            UpdateColumns(values);
            if (_TableCreated == false)
            {
                await CreateTable(tableName);
            }
            if (string.IsNullOrEmpty(_InsertCommand.CommandText))
            {
                _InsertCommand.CommandText = $"insert into [{_TableName}] ({string.Join(",", values.Keys)}) values ({string.Join(",", values.Keys.Select(c => $"@{c}").ToList())});";
            }
            AssignParameters(values, _InsertCommand);
            await _InsertCommand.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> Query()
        {
            return await Query(null, null);
        }

        public async Task<DataTable> Query(string filter)
        {
            return await Query(filter, null);
        }

        public async Task<SqliteDataReader> ExecuteReader(string filter = null, Dictionary<string, object> filterParameters = null, string orderBy = null)
        {
            SqliteCommand command = _Connection.CreateCommand();

            string order = string.IsNullOrEmpty(orderBy) ? string.Empty : $" order by {orderBy}";
            command.CommandText = $"select * from [{_TableName}] where {(string.IsNullOrEmpty(filter) ? "1=1" : filter)}{order}";

            if (filterParameters != null)
            {
                AssignParameters(filterParameters, command);
            }

            return await command.ExecuteReaderAsync();
        }

        public async Task<DataTable> Query(string filter = null, Dictionary<string, object> filterParameters = null)
        {
            SqliteDataReader reader = await ExecuteReader(filter, filterParameters);

            var fieldCount = reader.FieldCount;
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
            while (await reader.ReadAsync())
            {
                object[] values = new object[fieldCount];
                reader.GetValues(values);
                foreach (int boolIndex in boolColumns)
                {
                    values[boolIndex] = Convert.ToBoolean(Convert.ToInt32(values[boolIndex]));
                }
                dataTable.Rows.Add(values);
            }
            dataTable.Load(reader);
            return dataTable;
        }

        private async Task CreateTable(string tableName = "SqlData")
        {
            List<string> columns = new List<string>();

            foreach (SqlDataTableColumn column in Columns)
            {
                columns.Add($"{column.ColumnName} {SqlLiteType(column.DataType)}");
            }

            SqliteCommand command = _Connection.CreateCommand();
            command.CommandText = $"CREATE TABLE [{tableName}] ({string.Join(",", columns)});";
            await command.ExecuteNonQueryAsync();
            _TableCreated = true;
            _TableName = tableName;
        }

        private string SqlLiteType(Type dataType)
        {
            string sqlType = "TEXT";
            switch (dataType.Name)
            {
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
                case nameof(Char):
                    sqlType = "INTEGER";
                    break;
                case nameof(Decimal):
                case nameof(Double):
                case nameof(Single):
                case nameof(DateOnly):
                case nameof(DateTime):
                case nameof(TimeSpan):
                case nameof(TimeOnly):
                case nameof(DateTimeOffset):
                    sqlType = "REAL";
                    break;
                case nameof(Guid):
                    sqlType = "BLOB";
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

        private void AssignParameters(IDictionary parameters, SqliteCommand command)
        {
            foreach (string key in parameters.Keys)
            {
                SqliteParameter dbParam;

                if (command.Parameters.Contains(ParamName(key)))
                {
                    dbParam = command.Parameters[ParamName(key)];
                }
                else
                {
                    dbParam = command.CreateParameter();
                    dbParam.ParameterName = ParamName(key);
                    command.Parameters.Add(dbParam);
                }

                dbParam.Value = parameters[key] ?? DBNull.Value;
            }
        }

        private string ParamName(string key)
        {
            return key.StartsWith("@") ? key : $"@{key}";
        }

        public Dictionary<string, object> RecordToDictionary(object record, bool updateColumns = true)
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

        public Dictionary<string, object> DataRowToDictionary(DataRow dataRow, bool updateColumns = true)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                if (updateColumns)
                {
                    AddColumn(column.ColumnName, column.DataType);
                }
                dictionary[column.ColumnName] = dataRow[column.ColumnName];
            }

            return dictionary;
        }

        private bool IsSimple(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }

        private bool IsGenericList(object o)
        {
            var oType = o.GetType();
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }
    }
}
