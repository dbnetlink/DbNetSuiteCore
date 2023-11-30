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

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead || IsSimple(property.PropertyType) == false)
                {
                    continue;
                }

                _dataTable.Columns.Add(new DataColumn(property.Name, property.PropertyType));
            }
        }
        private void AddDataRow<T>(T record)
        {
            DataRow row = _dataTable.NewRow();
            Type type = record.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead || IsSimple(property.PropertyType) == false)
                {
                    continue;
                }

                row[property.Name] = property.GetValue(record, null);
            }
            _dataTable.Rows.Add(row);
        }

        private bool IsSimple(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }
    }
}
