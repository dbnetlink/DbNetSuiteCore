using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Enums.DbNetFile;
using DbNetSuiteCore.Models;
using System;
using System.Collections.Generic;

namespace DbNetSuiteCore.Components
{
    public class DbNetFileCoreColumn
    {
        readonly List<ColumnProperty> _columnProperties;
        readonly FileInfoProperties[] _columnTypes;

        internal DbNetFileCoreColumn(FileInfoProperties[] columnTypes, List<ColumnProperty> columnProperties)
        {
            _columnTypes = columnTypes;
            _columnProperties = columnProperties;
        }
        /// <summary>
        /// Sets the display format for the date/numeric column
        /// </summary>
        public DbNetFileCoreColumn Format(string format)
        {
            SetColumnProperty(ColumnPropertyType.Format, format);
            return this;
        }

        /// <summary>
        /// Sets the heading label for the column
        /// </summary>
        public DbNetFileCoreColumn Label(string label)
        {
            SetColumnProperty(ColumnPropertyType.Label, label);
            return this;
        }

        private void SetColumnProperty(Enum propertyType, object propertyValue)
        {
            foreach (FileInfoProperties columnType in _columnTypes)
            {
                _columnProperties.Add(new ColumnProperty(columnType.ToString(), propertyType, propertyValue));
            }
        }
    }
}