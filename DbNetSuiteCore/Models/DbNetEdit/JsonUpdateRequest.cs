using DbNetSuiteCore.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DbNetSuiteCore.Extensions;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Enums.DbNetEdit;
using System.Globalization;
using System.Threading;
using System;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class JsonUpdateRequest
    {
        private string _primaryKey;
        private Dictionary<string, object> _changes;
        public string PrimaryKey
        {
            set => _primaryKey = value;
        }
        public string PrimaryKeyName
        {
            get => primaryKey().Keys.First();
        }
        public object PrimaryKeyValue
        {
            get => ((JsonElement)primaryKey().Values.First()).Value();
        }
        public EditMode EditMode { get; set; }
        public Dictionary<string, object> Changes
        {
            get => convertChanges();
            set => _changes = value;
        }
        public object FormData { get; set; }
        public List<DbColumn> Columns { get; set; }

        private Dictionary<string, object> primaryKey()
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(EncodingHelper.Decode(_primaryKey));
        }

        private Dictionary<string, object> convertChanges()
        {
            Dictionary<string, object> convertedChanges = new Dictionary<string, object>();

            if (_changes != null)
            {
                foreach (string key in _changes.Keys)
                {
                    convertedChanges[key] = ConvertValueToType(key, ((JsonElement)_changes[key]).Value());
                }
            }

            return convertedChanges;
        }

        private object ConvertValueToType(string key, object value)
        {
            DbColumn column = this.Columns.FirstOrDefault(c => c.IsMatch(key));
            object paramValue = string.Empty;
            var dataType = column.DataType;

            try
            {
                switch (dataType)
                {
                    case nameof(Boolean):
                        if (value.ToString() == String.Empty)
                            paramValue = DBNull.Value;
                        else
                            paramValue = ParseBoolean(value.ToString());
                        break;
                    case nameof(TimeSpan):
                        paramValue = TimeSpan.Parse(DateTime.Parse(value.ToString()).ToString(column.Format));
                        break;
                    case nameof(DateTime):
                        if (string.IsNullOrEmpty(column.Format))
                        {
                            paramValue = Convert.ChangeType(value, Type.GetType($"System.{nameof(DateTime)}"));
                        }
                        else
                        {
                            if (column is EditColumn)
                            {
                                switch ((column as EditColumn).EditControlType)
                                {
                                    case EditControlType.DateTime:
                                    case EditControlType.Date:
                                        paramValue = DateTime.Parse(value.ToString(), null, DateTimeStyles.RoundtripKind);
                                        break;
                                    default:
                                        paramValue = DateTime.ParseExact(value.ToString(), column.Format, CultureInfo.CurrentCulture);
                                        break;
                                }
                            }
                            else
                            {
                                try
                                {
                                    paramValue = DateTime.ParseExact(value.ToString(), column.Format, CultureInfo.CurrentCulture);
                                }
                                catch
                                {
                                    paramValue = DateTime.Parse(value.ToString(), CultureInfo.CurrentCulture);
                                }
                            }
                        }
                        break;
                    case nameof(Byte):
                        paramValue = value;
                        break;
                    case nameof(Guid):
                        paramValue = new Guid(value.ToString());
                        break;
                    case nameof(Int16):
                    case nameof(Int32):
                    case nameof(Int64):
                    case nameof(Decimal):
                    case nameof(Single):
                    case nameof(Double):
                        if (string.IsNullOrEmpty(column.Format) == false)
                        {
                            var cultureInfo = Thread.CurrentThread.CurrentCulture;
                            value = value.ToString().Replace(cultureInfo.NumberFormat.CurrencySymbol, "");
                        }
                        paramValue = Convert.ChangeType(value, GetColumnType(dataType));
                        break;
                    case nameof(UInt16):
                    case nameof(UInt32):
                    case nameof(UInt64):
                        paramValue = Convert.ChangeType(value, GetColumnType(dataType.Replace("U", string.Empty)));
                        break;
                    default:
                        paramValue = Convert.ChangeType(value, GetColumnType(dataType));
                        break;
                }
            }
            catch 
            {
                return null;
            }

            return paramValue;
        }

        private Type GetColumnType(string typeName)
        {
            return Type.GetType("System." + typeName);
        }

        private int ParseBoolean(string boolString)
        {
            switch (boolString.ToLower())
            {
                case "true":
                case "1":
                    return 1;
                default:
                    return 0;
            }
        }
    };
}