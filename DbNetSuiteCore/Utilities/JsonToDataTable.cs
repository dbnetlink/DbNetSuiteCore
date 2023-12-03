using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DbNetSuiteCore.Utilities
{
    public class JsonToDataTable
    {
        private readonly DataTable _dataTable;
        private readonly string _json;
        private readonly Dictionary<string, Type> _columnDataTypes;
        public DataTable DataTable => _dataTable;

        public JsonToDataTable(string json, Dictionary<string, Type> columnDataTypes = null)
        {
            _dataTable = new DataTable();
            _json = json;
            _columnDataTypes = columnDataTypes;
            BuildDataTable();
        }

        private void BuildDataTable()
        {
            var array = JArray.Parse(_json);

            foreach (var jToken in array.First())
            {
                var jProperty = jToken as JProperty;
                _dataTable.Columns.Add(jProperty.Name, JsonColumnType(jProperty.Name));
            }

            try
            {
                foreach (var row in array)
                {
                    var datarow = _dataTable.NewRow();
                    foreach (var jToken in row)
                    {
                         var jProperty = jToken as JProperty;
                        if (_dataTable.Columns.Contains(jProperty.Name) == false)
                        {
                            _dataTable.Columns.Add(jProperty.Name, JsonColumnType(jProperty.Name));
                        }
                        if (jProperty.Value.IsNullOrEmpty() == false)
                        {
                            var type = JsonColumnType(jProperty.Name);
                            if (type == typeof(string))
                            {
                                datarow[jProperty.Name] = jProperty.Value;
                            }
                            else
                            {
                                datarow[jProperty.Name] = Convert.ChangeType(jProperty.Value.ToString(), type);
                            }
                        }
                        else
                        {
                            datarow[jProperty.Name] = DBNull.Value;
                        }
                    }
                    _dataTable.Rows.Add(datarow);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

        }

        private Type JsonColumnType(string name)
        {
            var type = typeof(string);

            if (_columnDataTypes != null)
            {
                string key = _columnDataTypes.Keys.FirstOrDefault(k => k.ToLower() == name.ToLower());
                if (key != null)
                {
                    type = _columnDataTypes[key];
                }
            }

            return type;
        }
    }

    public static class JTokenExtension
    {
        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
}
