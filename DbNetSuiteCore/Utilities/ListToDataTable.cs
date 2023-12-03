using DbNetSuiteCore.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DbNetSuiteCore.Utilities
{

    public class ListToDataTable
    {
        private readonly DataTable _dataTable;
        private Dictionary<string,Type> _propertyTypes;
        public DataTable DataTable => _dataTable;

        public ListToDataTable()
        {
            _dataTable = new DataTable();
        }

        public void AddList<T>(List<T> records)
        {
            AddDataColumns(records.First().GetType());

            foreach (T record in records)
            {
                AddDataRow(record);
            }
        }

        private void AddDataColumns(Type type)
        {
            _dataTable.TableName = type.Name;

            _propertyTypes = type.PropertyTypes();
            foreach (string name in _propertyTypes.Keys)
            {
                var propertyType = _propertyTypes[name];
                propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                _dataTable.Columns.Add(new DataColumn(name, propertyType));
            }
        }
        private void AddDataRow<T>(T record)
        {
            DataRow row = _dataTable.NewRow();
            Type type = record.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties.Where(p => _propertyTypes.Keys.Contains(p.Name)))
            {
                row[property.Name] = property.GetValue(record, null);
            }
            _dataTable.Rows.Add(row);
        }
    }
}
