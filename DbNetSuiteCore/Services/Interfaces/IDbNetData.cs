/*
	DbNetData is an open source library providing a common interface
    to the major database vendors.
	Copyright (C) 2011 Robin Coode - DbNetLink Limited
		
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	General Public License for more details.
	
	You should have received a copy of the GNU General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;

namespace DbNetSuiteCore.Services.Interfaces
{
    public interface IDbNetData
    {
        string ConnectionString { get; }
        DatabaseType Database { get; }
        int DatabaseVersion { get; }
        bool IsBatchUpdateSupported { get; }
        DataProvider Provider { get; }
        Hashtable ReservedWords { get; }
        int UpdateBatchSize { get; set; }

        event DbNetData.CommandConfiguredHandler OnCommandConfigured;

        void AddColumn(string TableName, string ColumnName, string ColumnDef);
        void AddColumn(string TableName, string ColumnName, string ColumnDef, object DefaultValue);
        void AddColumn(string TableName, string ColumnName, Type DataType);
        void AddColumn(string TableName, string ColumnName, Type DataType, int Length);
        void AddColumn(string TableName, string ColumnName, Type DataType, int Length, object DefaultValue);
        void AddColumn(string TableName, string ColumnName, Type DataType, object DefaultValue);
        void AddIndex(string TableName, string[] Columns);
        void AddTable(string TableName);
        void AddViewColumn(string ViewName, string ColumnExpression);
        void ApplyBatchUpdate();
        void BeginTransaction();
        void Close();
        bool ColumnExists(string TableName, string ColumnName);
        bool ColumnInReader(string ColumnName);
        bool ColumnIsNull(string ColumnName);
        void Commit();
        void CreateDatabase();
        ListDictionary DeriveParameters(string ProcedureName);
        void Dispose();
        long ExecuteDelete(CommandConfig CmdConfig);
        long ExecuteDelete(string Sql);
        long ExecuteDelete(string Sql, IDictionary Params);
        long ExecuteInsert(CommandConfig CmdConfig);
        long ExecuteInsert(string Sql);
        long ExecuteInsert(string Sql, IDictionary Params);
        int ExecuteNonQuery(CommandConfig CmdConfig);
        int ExecuteNonQuery(string Sql);
        int ExecuteNonQuery(string Sql, bool IgnoreErrors);
        int ExecuteNonQuery(string Sql, IDictionary Params);
        int ExecuteNonQuery(string Sql, IDictionary Params, bool IgnoreErrors);
        void ExecuteQuery(CommandConfig CmdConfig);
        void ExecuteQuery(QueryCommandConfig CmdConfig);
        void ExecuteQuery(string Sql);
        void ExecuteQuery(string Sql, IDictionary Params);
        void ExecuteQuery(string Sql, IDictionary Params, CommandBehavior Behaviour);
        object ExecuteScalar(CommandConfig Config);
        bool ExecuteSingletonQuery(QueryCommandConfig Query);
        bool ExecuteSingletonQuery(string Sql);
        bool ExecuteSingletonQuery(string Sql, IDictionary Params);
        bool ExecuteSingletonQuery(string Sql, IDictionary Params, CommandBehavior Behavior);
        long ExecuteUpdate(string Sql);
        long ExecuteUpdate(string Sql, IDictionary Params);
        long ExecuteUpdate(string Sql, IDictionary Params, IDictionary FilterParams);
        long ExecuteUpdate(UpdateCommandConfig CmdConfig);
        DataSet GetDataSet(QueryCommandConfig CmdConfig);
        DataSet GetDataSet(string Sql);
        DataSet GetDataSet(string Sql, IDictionary Params);
        DataTable GetDataTable(QueryCommandConfig CmdConfig);
        DataTable GetDataTable(string Sql);
        DataTable GetDataTable(string Sql, IDictionary Params);
        Hashtable GetHashtable();
        Hashtable GetHashtable(QueryCommandConfig Query);
        DataTable GetSchemaTable(string Sql);
        DataTable GetSchemaTable(string Sql, ListDictionary Params);
        long GetSequenceValue(string SequenceName, bool Increment);
        bool HasColumn(string columnName);
        bool IsReservedWord(string Token);
        ListDictionary JsonRecord();
        T Map<T>();
        List<T> MapToList<T>();
        DataTable MetaDataCollection(MetaDataType CollectionType);
        DataTable MetaDataCollection(string CollectionType);
        void Open(string ConnectionString, DataProvider? Provider, DatabaseType Database = DatabaseType.Unknown);
        string ParameterName(string Key);
        ListDictionary ParseParameters(string Sql);
        string QualifiedDbObjectName(string ObjectName);
        string ReaderString(int ColumnIndex);
        string ReaderString(string ColumnName);
        object ReaderValue(int ColumnIndex);
        object ReaderValue(string ColumnName);
        void Rollback();
        void RunScript(string Sql);
        void SetParamValue(ListDictionary Params, string ParamName, object ParamValue);
        bool TableExists(string TableName);
        string UnqualifiedDbObjectName(string ObjectName);
        string UnqualifiedDbObjectName(string ObjectName, bool BaseNameOnly);
        string UserTableFilter();
        string UserViewFilter();
        IDataReader GetDataReader();
        string CommandErrorInfo();
    }
}