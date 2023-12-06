#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion


using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using Newtonsoft.Json.Serialization;
using System.Linq;
using DbNetSuiteCore.Extensions;

namespace Newtonsoft.Json.Converters
{
     /// <summary>
    /// Converts a <see cref="DataTable"/> to and from JSON.
    /// </summary>
    public class TypedDataTableConverter : JsonConverter
    {
        private readonly Dictionary<string, Type> _columnDataTypes;

        public TypedDataTableConverter(Dictionary<string, Type> columnDataTypes) : base()
        {
            _columnDataTypes = columnDataTypes;
        }

        public TypedDataTableConverter(Type type) : base()
        {
            _columnDataTypes = GetColumnDataTypes(type);
        }
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            DataTable table = (DataTable)value;
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            writer.WriteStartArray();

            foreach (DataRow row in table.Rows)
            {
                writer.WriteStartObject();
                foreach (DataColumn column in row.Table.Columns)
                {
                    object columnValue = row[column];

                    if (serializer.NullValueHandling == NullValueHandling.Ignore && (columnValue == null || columnValue == DBNull.Value))
                    {
                        continue;
                    }

                    writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(column.ColumnName) : column.ColumnName);
                    serializer.Serialize(writer, columnValue);
                }
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (!(existingValue is DataTable dt))
            {
                // handle typed datasets
                dt = (objectType == typeof(DataTable))
                    ? new DataTable()
                    : (DataTable)Activator.CreateInstance(objectType)!;
            }

            // DataTable is inside a DataSet
            // populate the name from the property name
            if (reader.TokenType == JsonToken.PropertyName)
            {
                dt.TableName = (string)reader.Value!;

                reader.ReadAndAssert();

                if (reader.TokenType == JsonToken.Null)
                {
                    return dt;
                }
            }

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new Exception($"Unexpected JSON token when reading DataTable. Expected StartArray, got /{reader.TokenType}.");
            }

            reader.ReadAndAssert();

            while (reader.TokenType != JsonToken.EndArray)
            {
                CreateRow(reader, dt, serializer, _columnDataTypes);

                reader.ReadAndAssert();
            }

            return dt;
        }

        private static void CreateRow(JsonReader reader, DataTable dt, JsonSerializer serializer, Dictionary<string, Type> columnDataTypes)
        {
            DataRow dr = dt.NewRow();
            reader.ReadAndAssert();

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string columnName = (string)reader.Value!;

                reader.ReadAndAssert();

                DataColumn column = dt.Columns[columnName];
                if (column == null)
                {
                    Type columnType;
                    if (columnDataTypes.Keys.Contains(columnName.ToLower()))
                    {
                        columnType = columnDataTypes[columnName.ToLower()];
                    }
                    else
                    {
                        columnType = GetColumnDataType(reader);
                    }
                    column = new DataColumn(columnName, columnType);
                    dt.Columns.Add(column);
                }

                if (column.DataType == typeof(DataTable))
                {
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        reader.ReadAndAssert();
                    }

                    DataTable nestedDt = new DataTable();

                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        CreateRow(reader, nestedDt, serializer, columnDataTypes);

                       reader.ReadAndAssert();
                    }

                    dr[columnName] = nestedDt;
                }
                else if (column.DataType.IsArray && column.DataType != typeof(byte[]))
                {
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        reader.ReadAndAssert();
                    }

                    List<object> o = new List<object>();

                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        o.Add(reader.Value);
                   //     reader.ReadAndAssert();
                    }

                    Array destinationArray = Array.CreateInstance(column.DataType.GetElementType()!, o.Count);
                    ((IList)o).CopyTo(destinationArray, 0);

                    dr[columnName] = destinationArray;
                }
                else
                {
                    object columnValue = (reader.Value != null)
                        ? serializer.Deserialize(reader, column.DataType) ?? DBNull.Value
                        : DBNull.Value;

                    dr[columnName] = columnValue;
                }

                reader.ReadAndAssert();
            }

            dr.EndEdit();
            dt.Rows.Add(dr);
        }

        private static Type GetColumnDataType(JsonReader reader)
        {
            JsonToken tokenType = reader.TokenType;

            switch (tokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Boolean:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return reader.ValueType!;
                case JsonToken.Null:
                case JsonToken.Undefined:
                case JsonToken.EndArray:
                    return typeof(string);
                case JsonToken.StartArray:
                    reader.ReadAndAssert();
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        return typeof(DataTable); // nested datatable
                    }

                    Type arrayType = GetColumnDataType(reader);
                    return arrayType.MakeArrayType();
                default:
                    throw new Exception($"Unexpected JSON token when reading DataTable: {tokenType}");
            }
        }

        /// <summary>
        /// Determines whether this instance can convert the specified value type.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type valueType)
        {
            return typeof(DataTable).IsAssignableFrom(valueType);
        }

        private Dictionary<string, Type> GetColumnDataTypes(Type type)
        {
            Dictionary<string, Type> columnDataTypes = new Dictionary<string, Type>();
            var propertyTypes = type.PropertyTypes();
            foreach (string name in propertyTypes.Keys)
            {
                var propertyType = propertyTypes[name];
                propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                columnDataTypes[name.ToLower()] = propertyType;
            }

            return columnDataTypes;
        }
    }

    public static class JsonReaderExtension
    {
        public static void ReadAndAssert(this JsonReader reader)
        {
            if (!reader.Read())
            {
                throw new Exception("Unexpected end when reading JSON.");
            }
        }
    }
}
