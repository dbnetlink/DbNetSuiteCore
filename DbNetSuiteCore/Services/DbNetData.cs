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

// Support for OracleClient deprecated in .Net 4. Using reflection instead.
// using System.Data.OracleClient;
using DbNetSuiteCore.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
#if (!WINDOWS)

#endif

/// <summary>
/// The DbNetData.Data namespace encapsulates classes designed to provide a simplfied cross vendor interface to all the major database vendors.
/// </summary>
////////////////////////////////////////////////////////////////////////////
#if (WINDOWS)
namespace DbNetLink
#else
namespace DbNetSuiteCore.Services
#endif

////////////////////////////////////////////////////////////////////////////
{
    #region Enumerators
    /// <summary>
    /// The DataProvider enumerator identifies the .Net DataProvider to be used when connecting to a database with the <see cref="DbNetData.Open()"/> method
    /// </summary>

    public enum DataProvider
    {
        /// <summary>
        /// MS Sql Server Data Provider (default).
        /// </summary>
        SqlClient,

        /// <summary>
        /// MS Sql Server Compact Data Provider.
        /// </summary>
        SqlServerCE,

        /// <summary>
        /// Microsoft's Oracle Data Provider. Included with version 2.0 and above of the .Net Framework. 
        /// </summary>
        OracleClient,

        /// <summary>
        /// Generic ODBC Data Provider. Included with version 2.0 and above of the .Net Framework.
        /// </summary>		
        Odbc,

        /// <summary>
        /// Generic OleDb Data Provider.
        /// </summary>
        OleDb,

        /// <summary>
        /// Oracles's Data Provider.
        /// </summary>
        Oracle,

        /// <summary>
        /// Sybase Data Provider.
        /// </summary>
        Sybase,

        /// <summary>
        /// MySql Data Provider.
        /// </summary>
        MySql,

        /// <summary>
        /// CoreLabs MySql Data Provider.
        /// </summary>
        MyDirect,

        /// <summary>
        /// PgFoundry's PostgreSql Data Provider.
        /// </summary>
        Npgsql,

        /// <summary>
        /// Core Labs's PostgreSql Data Provider.
        /// </summary>
        PostgreSqlDirect,

        /// <summary>
        /// Firebird Data Provider.
        /// </summary>
        Firebird,

        /// <summary>
        /// Pervasive Data Provider.
        /// </summary>
        Pervasive,

        /// <summary>
        /// DB2 Data Provider. 
        /// </summary>
        DB2,

        /// <summary>
        /// VistaDB Data Provider.
        /// </summary>
        VistaDB,

        /// <summary>
        /// Sybase Data Provider from DataDirect.
        /// </summary>
        SybaseDataDirect,

        /// <summary>
        /// InterSystems Cache Data Provider.
        /// </summary>
        InterSystemsCache,

        /// <summary>
        /// Advanage Data Provider.
        /// </summary>
        Advantage,
        /// <summary>
        /// SQLite Data Provider
        /// </summary>
        SQLite
    };

    /// <summary>
    /// The DatabaseType enumerator identifies the database that is connected to. The enumerator is used for the <see cref="DbNetLink.Data.DbNetData.Database"/> property of the <see cref="DbNetLink.Data.DbNetData"/> class . The enumerator can also be specified on the DbNetData constructor <see cref="M:DbNetLink.Data.DbNetData.#ctor(System.String,DbNetLink.Data.DataProvider,DbNetLink.DatabaseType)"/> when it is not possible to automatically detect the database connected to.
    /// </summary>

    public enum DatabaseType
    {
        /// <summary>
        /// MS Access. <seealso href="http://office.microsoft.com/access/">Web site</seealso> 
        /// </summary>
        Access,
        /// <summary>
        /// MS Access(2007). <seealso href="http://www.microsoft.com/downloads/details.aspx?displaylang=en&amp;FamilyID=7554f536-8c28-4598-9b72-ef94e038c891">2007 Office Data Connectivity Components</seealso> 
        /// </summary>
        Access2007,
        /// <summary>
        /// Advantage Database Server. <seealso href="http://www.sybase.com/products/databasemanagement/advantagedatabaseserver">Web site</seealso> 
        /// </summary>
        Advantage,
        /// <summary>
        /// dBASE. <seealso href="http://www.dbase.com">Web site</seealso> 
        /// </summary>
        dBASE,
        /// <summary>
        /// IBM DB2. <seealso href="http://www.ibm.com/db2">Web site</seealso> 
        /// </summary>
        DB2,
        /// <summary>
        /// Excel spreadsheet 
        /// </summary>
        Excel,
        /// <summary>
        /// Excel(2007). <seealso href="http://www.microsoft.com/downloads/details.aspx?displaylang=en&amp;FamilyID=7554f536-8c28-4598-9b72-ef94e038c891">2007 Office Data Connectivity Components</seealso> 
        /// </summary>
        Excel2007,
        /// <summary>
        /// Firebird. <seealso href="http://www.firebirdsql.org">Web site</seealso> 
        /// </summary>
        Firebird,
        /// <summary>
        /// InterSystems Caché. <seealso href="http://www.intersystems.com/cache/">Web site</seealso> 
        /// </summary>
        InterSystemsCache,
        /// <summary>
        /// MySQL. <seealso href="http://www.mysql.com/">Web site</seealso> 
        /// </summary>
        MySql,
        /// <summary>
        /// Oracle. <seealso href="http://www.oracle.com/">Web site</seealso> 
        /// </summary>
        Oracle,
        /// <summary>
        /// Paradox
        /// </summary>
        Paradox,
        /// <summary>
        /// PostgreSQL. <seealso href="http://www.postgresql.org">Web site</seealso> 
        /// </summary>
        Pervasive,
        /// <summary>
        /// PostgreSQL. <seealso href="http://www.postgresql.org">Web site</seealso> 
        /// </summary>
        PostgreSql,
        /// <summary>
        /// Progress OpenEdge. <seealso href="http://www.progress.com/openedge">Web site</seealso> 
        /// </summary>
        Progress,
        /// <summary>
        /// SQLite. <seealso href="http://www.sqlite.org">Web site</seealso> 
        /// </summary>
        SQLite,
        /// <summary>
        /// MS SQL Server. <seealso href="http://www.microsoft.com/sql">Web site</seealso> 
        /// </summary>
        SqlServer,
        /// <summary>
        /// MS SQL Server Compact Edition.
        /// </summary>
        SqlServerCE,
        /// <summary>
        /// Sybase. <seealso href="http://www.sybase.com/products/databasemanagement/adaptiveserverenterprise">Web site</seealso> 
        /// </summary>
        Sybase,
        /// <summary>
        /// VistaDB. <seealso href="http://www.vistadb.net/">Web site</seealso> 
        /// </summary>
        TextFile,
        /// <summary>
        /// Plain text files in delimited of fixed width format 
        /// </summary>
        VistaDB,
        /// <summary>
        /// Visual FoxPro. <seealso href="http://msdn.microsoft.com/vfoxpro">Web site</seealso> 
        /// </summary>
        VisualFoxPro,
        Unknown
    };

    /// <summary>
    /// The MetaDataType enumerator identifies the meta data table that is returned when passed as a parameter to the <see cref="DbNetData.MetaDataCollection(MetaDataType)"/> method of the <see cref="DbNetData"/> class.
    /// </summary>

    public enum MetaDataType
    {
        /// <summary>
        /// Lists the meta data collections supported by the database
        /// </summary>
        MetaDataCollections,
        /// <summary>
        /// Lists the columns in the database
        /// </summary>
        Columns,
        /// <summary>
        /// Lists the databases
        /// </summary>
        Databases,
        /// <summary>
        /// Lists information about the database such as version number
        /// </summary>
        DataSourceInformation,
        /// <summary>
        /// Lists the data types supported by the database
        /// </summary>
        DataTypes,
        /// <summary>
        /// Lists the foreign keys the database
        /// </summary>
        ForeignKeys,
        /// <summary>
        /// Lists the user defined functions in the database (SQL Server only)
        /// </summary>
        Functions,
        /// <summary>
        /// Lists the index columns in the database
        /// </summary>
        IndexColumns,
        /// <summary>
        /// Lists the indexes in the database
        /// </summary>
        Indexes,
        /// <summary>
        /// Lists the primary keys in the database
        /// </summary>
        PrimaryKeys,
        /// <summary>
        /// Lists the stored procedures in the database
        /// </summary>
        Procedures,
        /// <summary>
        /// Lists the stored procedure parameters in the database
        /// </summary>
        ProcedureParameters,
        /// <summary>
        /// Lists the restrictions in the database
        /// </summary>
        Restrictions,
        /// <summary>
        /// Lists the reserved words in the database
        /// </summary>
        ReservedWords,
        /// <summary>
        /// Lists the tables in the database
        /// </summary>
        Tables,
        /// <summary>
        /// Lists the views in the database
        /// </summary>
        Views,
        /// <summary>
        /// Lists the view columns in the database
        /// </summary>
        ViewColumns,
        /// <summary>
        /// Lists the user defined types in the database
        /// </summary>
        UserDefinedTypes,
        /// <summary>
        /// Lists the users in the database
        /// </summary>
        Users,
        /// <summary>
        /// Lists the non-system tables in the database
        /// </summary>
        UserTables,
        /// <summary>
        /// Lists the non-system views in the database
        /// </summary>
        UserViews
    };
    #endregion

    #region CommandConfig
    /// <summary>
    /// The CommandConfig class is used to combine an SQL statment and its associated parameters
    /// into a single object
    /// </summary>
    /// <remarks>
    /// 	<para>The CommandConfig class can be used with the following methods</para>
    /// 	<list type="bullet">
    /// 		<item><see cref="DbNetLink.Data.DbNetData.ExecuteDelete(CommandConfig)"/></item>
    /// 		<item><see cref="DbNetLink.Data.DbNetData.ExecuteInsert(CommandConfig)"/></item>
    /// 		<item><see cref="DbNetLink.Data.DbNetData.ExecuteQuery(CommandConfig)"/></item>
    /// 		<item><see cref="DbNetLink.Data.DbNetData.ExecuteNonQuery(CommandConfig)"/></item>
    /// 		<item><see cref="DbNetLink.Data.DbNetData.ExecuteSingletonQuery(CommandConfig)"/></item>
    /// 		<item><see cref="DbNetLink.Data.DbNetData.ExecuteDelete(CommandConfig)"/></item>
    /// 	</list>
    /// </remarks>
    /// <example>
    /// <code>
    /// DbNetData Db = new DbNetData("Server=dbserver;Database=Northwind;Trusted_Connection=true;");
    /// CommandConfig CmdConfig = new CommandConfig("select * from customers where city = @city and country = @country");
    /// CmdConfig.Params["city"] = "London";
    /// CmdConfig.Params["country"] = "UK";
    /// Db.ExecuteQuery(CmdConfig);
    /// </code>
    /// </example>
    [Serializable]
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class CommandConfig
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    {
        /// <summary>
        /// Sql command text including any paramater placeholders
        /// </summary>
        public string Sql = String.Empty;
        /// <summary>
        /// Parameter values collection
        /// </summary>
        public ListDictionary Params = new ListDictionary();

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CommandConfig()
            : this("")
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
        }

        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CommandConfig(string Sql)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            this.Sql = Sql;
        }
    }
    #endregion

    #region QueryCommandConfig
    /// <summary>
    /// The QueryCommandConfig class is used to combine an SQL statment, associated parameters
    /// and CommandBehavior into a single object
    /// </summary>
    /// <remarks>
    /// 	<para>The QueryCommandConfig object can be used with the following methods</para>
    /// 	<list type="bullet">
    /// 		<item><see cref="DbNetData.ExecuteQuery(QueryCommandConfig)"/></item>
    /// 		<item><see cref="DbNetData.GetDataSet(QueryCommandConfig)"/></item>
    /// 		<item><see cref="DbNetData.GetDataTable(QueryCommandConfig)"/></item>
    /// 	</list>
    /// </remarks>
    /// <example>
    /// <code>
    /// DbNetData Db = new DbNetData("Server=dbserver;Database=Northwind;Trusted_Connection=true;");
    /// QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from customers where city = @city and country = @country");
    /// CmdConfig.Params["city"] = "London";
    /// CmdConfig.Params["country"] = "UK";
    /// CmdConfig.Behavior = CommandBehavior.SequentialAccess;
    /// Db.ExecuteQuery(CmdConfig);
    /// </code>
    /// </example>

    [Serializable]
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class QueryCommandConfig : CommandConfig
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    {
        /// <summary>
        /// The QueryCommandConfig class is used to combine an SQL statment, associated parameters
        /// and CommandBehavior into a single object
        /// </summary>
        public CommandBehavior Behavior = CommandBehavior.Default;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public QueryCommandConfig()
            : this("")
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
        }
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public QueryCommandConfig(string Sql)
            : base(Sql)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
        }
    }

    /// <summary>
    /// The UpdateCommandConfig class is used to combine an SQL statment, associated update parameters
    /// and filter parameters into a single object
    /// </summary>
    /// <remarks>
    /// 	<para>The UpdateCommandConfig object can be used with the following methods</para>
    /// 	<list type="bullet">
    /// 		<item><see cref="DbNetData.ExecuteUpdate(UpdateCommandConfig)"/></item>
    /// 	</list>
    /// </remarks>
    /// <example>
    /// <code>
    /// DbNetData Db = new DbNetData("Server=dbserver;Database=Northwind;Trusted_Connection=true;");
    /// UpdateCommandConfig CmdConfig = new UpdateCommandConfig("update products set discontinued = @discontinued where categoryid = @categoryid");
    /// CmdConfig.Params["discontinued"] = 1;
    /// CmdConfig.FilterParams["categoryid"] = 9;
    /// // Discontinue all products that have a Category Id of 9
    /// Db.ExecuteUpdate(CmdConfig);
    /// </code>
    /// </example>
    /// 
    #endregion

    #region UpdateCommandConfig

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class UpdateCommandConfig : CommandConfig
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    {
        /// <summary>
        /// Parameters used to create the "where" filter on the update statement
        /// </summary>
        public ListDictionary FilterParams = new ListDictionary();

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public UpdateCommandConfig()
            : this("")
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
        }
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public UpdateCommandConfig(string Sql)
            : base(Sql)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
        }
    }
    #endregion


    #region DbNetData
    /// <summary>
    /// The DbNetData class is used to encapsulate a database connection with a collection of methods and properties 
    /// that use the connection.
    /// </summary>

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    public class DbNetData : IDisposable, IDbNetData
    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    {
        #region Delegates
        /// <summary>
        /// Delegate definition for the <see cref="DbNetData.OnCommandConfigured"/> event.
        /// </summary>
        public delegate void CommandConfiguredHandler(DbNetData ConnectonInstance, IDbCommand Command);
        #endregion

        #region Events
        /// <summary>
        /// OnCommandConfigured event can be used to customise the configuration of the command just prior to it being executed.
        /// </summary>
        public event CommandConfiguredHandler OnCommandConfigured = null;
        #endregion

        #region Public Properties
        /// <summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/system.data.IDbDataAdapter.aspx">IDbDataAdapter</seealso> implemented by provider
        /// </summary>
        public IDbDataAdapter Adapter;
        /// <summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/system.data.IDbConnection.aspx">IDbConnection</seealso> implemented by provider
        /// </summary>
        public IDbConnection Conn = null;
        /// <summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/system.data.IDbCommand.aspx">IDbCommand</seealso> implemented by provider
        /// </summary>
        public IDbCommand Command;
        /// <summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/system.data.IDataReader.aspx">IDataReader</seealso> implemented by provider
        /// </summary>
        public IDataReader _Reader;
        public IDataReader Reader => _Reader;
        public IDataReader GetDataReader()
        {
            return _Reader;
        }
        /// <summary>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/system.data.IDbTransaction.aspx">IDbTransaction</seealso> implemented by provider
        /// </summary>
        public IDbTransaction Transaction;
        /// <summary>
        /// If set to true then the connection is automatically closed if an error occurs
        /// </summary>
        public bool CloseConnectionOnError = true;
        /// <summary>
        /// The number of seconds before a command will timeout. 30 is the default. A value of 0 will prevent a timeout.
        /// </summary>
        public int CommandTimeout = -1;
        /// <summary>
        /// The connection string (after any path mappings have been made)
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
        }
        /// <summary>
        /// The type of database connected to
        /// </summary>
        /// <remarks>
        /// Property is read-only. To assign the database type it must be passed as a parameter to the <see cref="DbNetData(string,MFLDirect.CustomerSupport.Web.Helpers.DataProvider,MFLDirect.CustomerSupport.Web.Helpers.DatabaseType)">constructor</see>
        /// </remarks>
        public DatabaseType Database
        {
            get { return _Database; }
        }
        /// <summary>
        /// The data provider used for the connection
        /// </summary>
        /// <remarks>
        /// Property is read-only. To assign the provider type it must be passed as a parameter to the <see cref="DbNetData(string,MFLDirect.CustomerSupport.Web.Helpers.DataProvider)">constructor</see>
        /// </remarks>
        public DataProvider Provider
        {
            get { return _Provider; }
        }
        /// <summary>
        /// The autoincrementing value assigned to the last inserted record (where applicable). Only set if <see cref="DbNetData.ReturnAutoIncrementValue"/> is set to true
        /// </summary>
        public long Identity = -1;
        /// <summary>
        /// The threshold in milliseconds above which details of commands executing for longer will be logged
        /// </summary>
        public int CommandDurationWarningThreshold = 100;
        /// <summary>
        /// The template used to qualify database object names containing spaces
        /// </summary>
        public string NameDelimiterTemplate = "{0}";
        /// <summary>
        /// Determines if an attempt is made to fetch the value assigned by an autoincrementing column after an insert
        /// </summary>
        public bool ReturnAutoIncrementValue = false;
        /// <summary>
        /// The number of rows affected by the last <see cref="DbNetData.ExecuteDelete(string)"/>  or <see cref="DbNetData.ExecuteUpdate(string)"/> method
        /// </summary>
        public long RowsAffected;
        /// <summary>
        /// The fully mapped path to the data source
        /// </summary>
        public string DataSourcePath = "";
        /// <summary>
        /// Controls the verbosity of the error message
        /// </summary>
        public bool VerboseErrorInfo = true;
        /// <summary>
        /// Prevents accidental execution of unqualified update/delete statements
        /// </summary>
        public bool AllowUnqualifiedUpdates = false;
        /// <summary>
        /// Controls the level of detail shown in any exceptions
        /// </summary>
        public bool SummaryExceptionMessage = false;
        /// <summary>
        /// Converts empty string to null values in update statements when set to true
        /// </summary>
        public bool ConvertEmptyToNull = true;
        /// <summary>
        /// Controls the display of the connection string when reporting an error condition
        /// </summary>
        public bool ShowConnectionStringOnError = false;
        /// <summary>
        /// Indicates that all database object names should be qualified.
        /// </summary>
        public bool QualifyAllObjectNames = false;
        /// <summary>
        /// If true will automatically upgrade a SQL Server CE database.
        /// </summary>
        public bool UpgradeSQLServerCE = false;
        /// <summary>
        /// Specifies the batch size for batch updates.
        /// </summary>
        public int UpdateBatchSize
        {
            get { return _UpdateBatchSize; }
            set { if (this.IsBatchUpdateSupported) _UpdateBatchSize = value; }
        }
        /// <summary>
        /// Returns a list of reserved words for the database
        /// </summary>
        public Hashtable ReservedWords
        {
            get { return GetReservedWords(); }
        }
        /// <summary>
        /// Returns the major version number for the database
        /// </summary>
        public int DatabaseVersion
        {
            get { return GetDatabaseVersion(); }
        }
        /// <summary>
        /// Indicates if database provider supports batch updates
        /// </summary>
        public bool IsBatchUpdateSupported
        {
            get { return _BatchUpdateSupported; }
        }
        #endregion

        #region Private Properties
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        private Hashtable _ReservedWords = new Hashtable();

        private DateTime CommandStart;
        private DataRow DataSourceInfo = null;
        internal Assembly ProviderAssembly;
        internal string ParameterTemplate = "@{0}";
        private DataTable InsertsTable = null;

        private DatabaseType _Database = DatabaseType.Unknown;
        private DataProvider _Provider = DataProvider.SqlClient;
        private string _ConnectionString = "";
        private int _Vn = System.Int32.MinValue;
        private bool _BatchUpdateSupported = false;
        private int _UpdateBatchSize = 1;
        private string _BatchInsertSelectSql = "";

        #endregion

        #region Constructors

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public DbNetData(IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            this._httpContextAccessor = httpContextAccessor;
            this._hostingEnvironment = hostingEnvironment;
            this._configuration = configuration;
        }
        /*
            public DbNetData(string ConnectionString, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
    : this(ConnectionString, DataProvider.SqlClient, DatabaseType.Unknown, httpContextAccessor, hostingEnvironment, configuration)
        {
        }

        public DbNetData(string ConnectionString, DataProvider? Provider, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
            : this(ConnectionString, Provider, DatabaseType.Unknown, httpContextAccessor, hostingEnvironment, configuration)
        {
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public DbNetData(string ConnectionString, DataProvider? Provider, DatabaseType Database, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            this._httpContextAccessor = httpContextAccessor;
            this._hostingEnvironment = hostingEnvironment;
            this._configuration = configuration;
        }
        */
        #endregion

        #region Public Methods

      
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        ~DbNetData()
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Dispose(false);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void Dispose(bool disposing)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (disposing)
            {
                Close();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddColumn(string TableName, string ColumnName, Type DataType, object DefaultValue)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            AddColumn(TableName, ColumnName, DataType, 50, DefaultValue);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddColumn(string TableName, string ColumnName, Type DataType)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            AddColumn(TableName, ColumnName, DataType, 50, null);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddColumn(string TableName, string ColumnName, Type DataType, int Length)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            AddColumn(TableName, ColumnName, DataType, Length, null);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddColumn(string TableName, string ColumnName, Type DataType, int Length, object DefaultValue)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string ColumnDef = "";

            switch (DataType.Name)
            {
                case "Boolean":
                    switch (Database)
                    {
                        case DatabaseType.SqlServer:
                        case DatabaseType.SqlServerCE:
                            ColumnDef = "Bit";
                            break;
                        case DatabaseType.Access:
                        case DatabaseType.Access2007:
                            ColumnDef = "YesNo";
                            break;
                    }
                    break;
                case "Int16":
                case "Int32":
                case "Int64":
                    switch (Database)
                    {
                        case DatabaseType.SqlServer:
                        case DatabaseType.SqlServerCE:
                            ColumnDef = "int";
                            break;
                        case DatabaseType.Access:
                        case DatabaseType.Access2007:
                            ColumnDef = "short";
                            break;
                    }
                    break;
                case "String":
                    switch (Database)
                    {
                        case DatabaseType.SqlServer:
                        case DatabaseType.SqlServerCE:
                            if (Length <= 4000)
                                ColumnDef = "nvarchar(" + Length.ToString() + ")";
                            else
                                ColumnDef = "ntext";
                            break;
                        case DatabaseType.Access:
                        case DatabaseType.Access2007:
                            if (Length <= 255)
                                ColumnDef = "text(" + Length.ToString() + ")";
                            else
                                ColumnDef = "memo";
                            break;
                    }
                    break;

            }

            if (ColumnDef == "")
                throw new Exception("AddColumn ==> Column type not handled ==>" + DataType.Name + " ==>" + Database.ToString());
            else
                AddColumn(TableName, ColumnName, ColumnDef, DefaultValue);
        }

        /// <summary>
        /// Adds a column (if it does not already exist) to an existing SQL server table 
        /// </summary>
        /// <param name="TableName">
        ///     <para>
        ///        The name of the table
        ///     </para>
        /// </param>
        /// <param name="ColumnName">
        ///     <para>
        ///        The name of the column
        ///     </para>
        /// </param>
        /// <param name="ColumnDef">
        ///     <para>
        ///        The column definition
        ///     </para>
        /// </param>        
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( "DbNetTime" );
        /// Db.Open();
        /// Db.AddColumn("dbnetbug_task_user", "task_id", "int");
        /// Db.AddColumn("dbnetbug_task_user", "user_id", "int");
        /// Db.AddColumn("dbnetbug_task_user", "hourly_rate", "real");
        /// Db.Close();		
        /// </code>
        /// </example>        
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddColumn(string TableName, string ColumnName, string ColumnDef)
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            AddColumn(TableName, ColumnName, ColumnDef, null);
        }

        /// <summary>
        /// Adds a column to an existing SQL server table and assigns a default value
        /// </summary>
        /// <param name="TableName">
        ///     <para>
        ///        The name of the table
        ///     </para>
        /// </param>
        /// <param name="ColumnName">
        ///     <para>
        ///        The name of the column
        ///     </para>
        /// </param>
        /// <param name="ColumnDef">
        ///     <para>
        ///        The column definition
        ///     </para>
        /// </param>  
        /// <param name="DefaultValue">
        ///     <para>
        ///        The default value for the columns
        ///     </para>
        /// </param>    
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( "DbNetTime" );
        /// Db.Open();
        /// Db.AddColumn("dbnetbug_config", "singlestep_quickentry", "bit", 1);
        /// Db.AddColumn("dbnetbug_user", "supplies_start_end", "bit", 0);
        /// Db.Close();		
        /// </code>
        /// </example>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddColumn(string TableName, string ColumnName, string ColumnDef, object DefaultValue)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (ColumnExists(TableName, ColumnName))
                return;

            switch (this.Database)
            {
                case DatabaseType.SqlServerCE:
                    ColumnDef = Regex.Replace(ColumnDef, "^varchar", "nvarchar", RegexOptions.IgnoreCase);
                    break;
                case DatabaseType.Oracle:
                    ColumnDef = Regex.Replace(ColumnDef, "^varchar", "varchar2", RegexOptions.IgnoreCase);
                    ColumnDef = Regex.Replace(ColumnDef, "^text", "clob", RegexOptions.IgnoreCase);
                    ColumnDef = Regex.Replace(ColumnDef, "^datetime", "date", RegexOptions.IgnoreCase);
                    break;
                case DatabaseType.Access:
                case DatabaseType.Access2007:
                    ColumnDef = Regex.Replace(ColumnDef, "^text", "memo", RegexOptions.IgnoreCase);
                    ColumnDef = Regex.Replace(ColumnDef, "bit", "yesno", RegexOptions.IgnoreCase);
                    ColumnDef = Regex.Replace(ColumnDef, "int", "short", RegexOptions.IgnoreCase);
                    ColumnDef = Regex.Replace(ColumnDef, "^varchar", "text", RegexOptions.IgnoreCase);
                    break;
                case DatabaseType.DB2:
                    ColumnDef = Regex.Replace(ColumnDef, "^text", "varchar(8000)", RegexOptions.IgnoreCase);
                    break;
                case DatabaseType.PostgreSql:
                    ColumnDef = Regex.Replace(ColumnDef, "^varchar", "character varying", RegexOptions.IgnoreCase);
                    break;
            }

            CommandConfig Alter = new CommandConfig();

            switch (this.Database)
            {
                case DatabaseType.SqlServerCE:
                case DatabaseType.SqlServer:
                    Alter.Sql = "alter table " + TableName + " add " + ColumnName + " " + ColumnDef;
                    if (DefaultValue != null)
                    {
                        string Quote = String.Empty;
                        if (DefaultValue is string)
                            Quote = "'";

                        Alter.Sql += " DEFAULT ((" + Quote + DefaultValue.ToString() + Quote + "))";
                    }
                    break;
                case DatabaseType.Access:
                case DatabaseType.Access2007:
                    Alter.Sql = "alter table " + TableName + " add column " + ColumnName + " " + ColumnDef;
                    break;
                case DatabaseType.Oracle:
                    Alter.Sql = "alter table " + TableName + " add " + ColumnName + " " + ColumnDef;
                    break;
                default:
                    Alter.Sql = "alter table " + TableName + " add " + ColumnName + " " + ColumnDef;
                    break;
            }

            this.ExecuteNonQuery(Alter);

            if (DefaultValue != null)
            {
                UpdateCommandConfig UpdateValue = new UpdateCommandConfig();
                UpdateValue.Sql = "update " + TableName + " set " + ColumnName + " = @default_value where 1=1";
                UpdateValue.Params["default_value"] = DefaultValue;
                this.ExecuteUpdate(UpdateValue);
            }
        }
        /// <summary>
        /// Checks for the existence of a column in a table
        /// </summary>
        /// <param name="TableName">
        ///     <para>
        ///        The name of the table
        ///     </para>
        /// </param>
        /// <param name="ColumnName">
        ///     <para>
        ///        The name of the column
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns true if the column exists
        /// </returns> 
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool ColumnExists(string TableName, string ColumnName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            bool ColumnExists = true;
            try
            {
                this.CloseConnectionOnError = false;
                QueryCommandConfig Query = new QueryCommandConfig();
                Query.Sql = "select " + ColumnName + " from " + TableName + " where 1=2";
                this.ExecuteSingletonQuery(Query);
            }
            catch (Exception)
            {
                ColumnExists = false;
            }
            CloseReader();

            return ColumnExists;
        }

        /// <summary>
        /// Adds a table to the database if it does not alreay exist (SQL Server only). The table is created 
        /// with a single IDENTITY column named ID. Additonal columns can be added with <see cref="DbNetData.AddColumn(string,string,string)"/>.
        /// </summary>
        /// <param name="TableName">
        ///     <para>
        ///        The name of the table
        ///     </para>
        /// </param>

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddTable(string TableName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (TableExists(TableName))
                return;

            string Sql = "";

            switch (this.Database)
            {
                case DatabaseType.SqlServer:
                    Sql = "create table " + TableName + "([id] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_" + TableName + "] PRIMARY KEY CLUSTERED ([id] ASC))";
                    break;
                case DatabaseType.SqlServerCE:
                    Sql = "create table " + TableName + "([id] [int] IDENTITY(1,1) PRIMARY KEY)";
                    break;
                case DatabaseType.Access:
                case DatabaseType.Access2007:
                    Sql = "create table " + TableName + "([id] COUNTER PRIMARY KEY);";
                    break;
                case DatabaseType.MySql:
                    Sql = "CREATE TABLE " + TableName + "(id INT NOT NULL AUTO_INCREMENT PRIMARY KEY)";
                    break;
                case DatabaseType.Oracle:
                    Sql = "create table " + TableName + "(id NUMBER PRIMARY KEY)~";
                    Sql += "CREATE SEQUENCE " + TableName + "_s START WITH 1 INCREMENT BY 1~";
                    Sql += "CREATE OR REPLACE TRIGGER " + TableName + "_t ";
                    Sql += "BEFORE INSERT ";
                    Sql += "ON " + TableName + " ";
                    Sql += "FOR EACH ROW ";
                    Sql += "BEGIN ";
                    Sql += "SELECT " + TableName + "_s.nextval INTO :NEW.ID FROM dual; END;";
                    break;
                case DatabaseType.DB2:
                    Sql = "CREATE TABLE " + TableName + "(id INT NOT NULL GENERATED ALWAYS AS IDENTITY(START WITH 1, INCREMENT BY 1) )";
                    break;
                case DatabaseType.PostgreSql:
                    Sql = "CREATE TABLE " + TableName + " (id serial NOT NULL, CONSTRAINT " + TableName + "_pk PRIMARY KEY (id) ) WITHOUT OIDS;";
                    break;
            }

            foreach (string SqlStmnt in Sql.Split('~'))
                this.ExecuteNonQuery(SqlStmnt);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddIndex(string TableName, string[] Columns)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string SqlStmnt = "create index " + TableName + "_idx on " + TableName + "(" + String.Join(",", Columns) + ")";
            this.ExecuteNonQuery(SqlStmnt, true);
        }

        /// <summary>
        /// Checks for the existence of a table in the database
        /// </summary>
        /// <param name="TableName">
        ///     <para>
        ///        The name of the table
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns true if the table exists
        /// </returns> 
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool TableExists(string TableName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            bool TableExists = true;
            try
            {
                this.CloseConnectionOnError = false;
                QueryCommandConfig Query = new QueryCommandConfig();
                Query.Sql = "select * from " + TableName + " where 1=2";
                this.ExecuteSingletonQuery(Query);
            }
            catch (Exception)
            {
                TableExists = false;
            }
            CloseReader();
            return TableExists;
        }
        /// <summary>
        /// Adds a column (if it does not already exist) to an existing SQL Server View (SQL Server Only) 
        /// </summary>
        /// <param name="ViewName">
        ///     <para>
        ///        The name of the view
        ///     </para>
        /// </param>
        /// <param name="ColumnExpression">
        ///     <para>
        ///        The expression representing the column
        ///     </para>
        /// </param>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( "DbNetTime" );
        /// Db.Open();
        /// Db.AddViewColumn("dbnetbug_hours_view", "h.hourly_rate as hourly_rate");
        /// Db.AddViewColumn("dbnetbug_hours_view", "(h.hourly_rate * h.decimal_time) as hours_value");
        /// Db.Close();		
        /// </code>
        /// </example>    
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddViewColumn(string ViewName, string ColumnExpression)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (this.Database == DatabaseType.SqlServerCE)
                return;

            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = "select view_definition from information_schema.views where table_name = @view_name";
            Query.Params["view_name"] = ViewName;

            if (!this.ExecuteSingletonQuery(Query))
                return;

            String ViewDefinition = this.ReaderValue("view_definition").ToString().Replace(System.Environment.NewLine, " ");
            Match M = Regex.Match(ViewDefinition, @"select (.*?)(from .*)", RegexOptions.IgnoreCase);

            String[] CurrentColumns = M.Groups[1].ToString().Split(',');
            string FromPart = M.Groups[2].ToString();

            List<string> NewColumns = new List<string>();
            bool ColumnAdded = false;

            foreach (string ColumnName in CurrentColumns)
            {
                if (ColumnName.Trim().Equals(ColumnExpression, StringComparison.CurrentCultureIgnoreCase))
                    return;

                if (ColumnName.Trim().Split('.')[ColumnName.Split('.').Length - 1].StartsWith("ud_") && !ColumnAdded)
                {
                    NewColumns.Add(ColumnExpression);
                    ColumnAdded = true;
                }

                NewColumns.Add(ColumnName);
            }

            if (!ColumnAdded)
                NewColumns.Add(ColumnExpression);

            string Sql = "alter view " + ViewName + " as select " + String.Join(",", NewColumns.ToArray()) + System.Environment.NewLine + FromPart;
            this.ExecuteNonQuery(Sql);
        }


        /// <summary>
        /// Applys the batch of updates
        /// </summary>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void ApplyBatchUpdate()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (this.InsertsTable == null)
                return;

            switch (this.Provider)
            {
                case DataProvider.SqlClient:
                    ((SqlDataAdapter)Adapter).Update(this.InsertsTable);
                    break;
                case DataProvider.MySql:
                case DataProvider.DB2:
                case DataProvider.OracleClient:
                    Object[] Args = new Object[1];
                    Args[0] = this.InsertsTable;
                    InvokeMethod(Adapter, "Update", Args);
                    break;
                // case DataProvider.OracleClient:
                //     ((OracleDataAdapter)Adapter).Update(this.InsertsTable);
                //     break;
                default:
                    throw new Exception("Batch update not supported by this data provider");
                    break;
            }

            this.InsertsTable.Rows.Clear();
        }

        /// <summary>
        /// Returns a boolean indicating id the value for the specified column name is null
        /// </summary>
        /// <param name="ColumnName">
        ///     <para>
        ///        The name of the column
        ///     </para>
        /// </param>
        /// <returns>
        /// bool
        /// </returns>

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool ColumnIsNull(string ColumnName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                return _Reader.IsDBNull(_Reader.GetOrdinal(ColumnName));
            }
            catch (Exception)
            {
                throw new Exception("ReaderValue column not found: " + ColumnName);
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool ColumnInReader(string ColumnName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            for (int i = 0; i < _Reader.FieldCount; i++)
                if (_Reader.GetName(i).Equals(ColumnName, StringComparison.InvariantCultureIgnoreCase)) return true;

            return false;
        }


        /// <summary>
        /// Starts a database transaction
        /// </summary>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void BeginTransaction()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Transaction = Conn.BeginTransaction();
            Command.Transaction = Transaction;
        }

        /// <summary>
        /// Rolls back a database transaction
        /// </summary>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Rollback()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Transaction.Rollback();
        }

        /// <summary>
        /// Commits a database transaction
        /// </summary>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Commit()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            try
            {

                Transaction.Commit();
            }
            catch (Exception)
            {
            }
        }

        public T Map<T>()
        {
            return MapToList<T>().FirstOrDefault();
        }

        public List<T> MapToList<T>()
        {
            List<T> list = new List<T>();
            T obj = default(T);
            while (_Reader.Read())
            {
                obj = Activator.CreateInstance<T>();
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (HasColumn(prop.Name))
                    {
                        if (!object.Equals(_Reader[prop.Name], DBNull.Value))
                        {
                            prop.SetValue(obj, _Reader[prop.Name], null);
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        public bool HasColumn(string columnName)
        {
            for (int i = 0; i < _Reader.FieldCount; i++)
            {
                if (_Reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Close()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            CloseReader();
            if (Conn.State != ConnectionState.Closed)
            {
                Conn.Close();
            }
        }

        /// <summary>
        /// Creates a database using the supplied connection string
        /// </summary>
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void CreateDatabase()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Object[] Args;

            switch (Database)
            {
                case DatabaseType.SqlServerCE:
                    Type SqlCeEngineType = ProviderAssembly.GetType("System.Data.SqlServerCe.SqlCeEngine", true);
                    Args = new Object[1];
                    Args[0] = this.ConnectionString;

                    object SqlCeEngine = Activator.CreateInstance(SqlCeEngineType, Args);

                    InvokeMethod(SqlCeEngine, "CreateDatabase");
                    InvokeMethod(SqlCeEngine, "Dispose");
                    break;
                case DatabaseType.SQLite:
                    Args = new Object[1];
                    Args[0] = DataSourceFileName();
                    InvokeMethod(Conn, "CreateFile", Args);
                    break;
                default:
                    throw new Exception("Not supported for this database type");
                    break;
            }
        }

        /// <summary>
        /// Parses the Sql statement for parameter names and returns a corresponding paramater collection 
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns a collection of parameter objects for each parameter in the SQL
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// string Sql = "select first_name from user where userid = @userid";
        /// ListDictionary Params = Data.ParseParameters(Sql);
        /// Params["userid"] = Users.SelectedValue;
        /// Db.ExecuteQuery( Sql, Params );
        /// Db.Close();		
        /// </code>
        /// </example>

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ListDictionary ParseParameters(string Sql)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            ListDictionary Params = new ListDictionary();
            MatchCollection MC = Regex.Matches(Sql, @"[@:](\w*)");

            foreach (Match M in MC)
                Params[M.Groups[1].Value] = new object();

            return Params;
        }


        public static List<T> MapDataToBusinessEntityCollection<T>(IDataReader dr) where T : new()
        {
            Type businessEntityType = typeof(T);
            List<T> entitys = new List<T>();
            Hashtable hashtable = new Hashtable();
            PropertyInfo[] properties = businessEntityType.GetProperties();
            foreach (PropertyInfo info in properties)
            {
                hashtable[info.Name.ToUpper()] = info;
            }
            while (dr.Read())
            {
                T newObject = new T();
                for (int index = 0; index < dr.FieldCount; index++)
                {
                    PropertyInfo info = (PropertyInfo)hashtable[dr.GetName(index).ToUpper()];
                    if ((info != null) && info.CanWrite)
                    {
                        info.SetValue(newObject, dr.GetValue(index), null);
                    }
                }
                entitys.Add(newObject);
            }
            dr.Close();
            return entitys;
        }

        /// <summary>
        /// Derives the parameters for a stored procedure
        /// </summary>
        /// <param name="ProcedureName">
        ///     <para>
        ///        The name of the stored procedure
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns a collection of IDbDataParameter objects for each parameter in the stored procedure
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDictionary Params = Db.DeriveParameters("CustOrderHist");
        /// ((IDbDataParameter)Params["customerid"]).Value = Customers.SelectedValue;
        /// Db.ExecuteQuery( "CustOrderHist", Params );
        /// ResultsGridView.DataSource = Db.Reader;
        /// ResultsGridView.DataBind();
        /// Db.Close();		
        /// </code>
        /// </example>

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ListDictionary DeriveParameters(string ProcedureName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string TypeName = "";

            switch (Provider)
            {
                case DataProvider.OleDb:
                    TypeName = "OleDbCommandBuilder";
                    break;
                case DataProvider.Odbc:
                    TypeName = "OdbcCommandBuilder";
                    break;
                case DataProvider.OracleClient:
                case DataProvider.Oracle:
                    TypeName = "OracleCommandBuilder";
                    break;
                case DataProvider.SqlClient:
                    TypeName = "SqlCommandBuilder";
                    break;
                case DataProvider.VistaDB:
                    TypeName = "VistaDBCommandBuilder";
                    break;
                default:
                    throw new Exception("DeriveParameters not supported by this provider");
            }

            string[] TypeNameParts = Conn.GetType().FullName.Split('.');

            TypeNameParts[TypeNameParts.Length - 1] = TypeName;
            string CommandBuilderTypeName = string.Join(".", TypeNameParts);

            Type CommandBuilder = ProviderAssembly.GetType(CommandBuilderTypeName, true);

            Type[] TypeArray = new Type[1];
            TypeArray.SetValue(ProviderAssembly.GetType(CommandBuilderTypeName.Replace("Builder", ""), true), 0);

            MethodInfo MI = CommandBuilder.GetMethod("DeriveParameters", TypeArray);

            if (MI == null)
                throw new Exception("Method --> DeriveParameters not supported by --> " + string.Join(".", TypeNameParts) + " " + CommandBuilder.GetType().ToString());

            Object[] Args = new Object[1];

            Command.CommandText = ProcedureName;
            Command.CommandType = CommandType.StoredProcedure;

            Args[0] = Command;

            MI.Invoke(Conn, Args);

            ListDictionary Params = new ListDictionary();

            foreach (IDbDataParameter DbParam in Command.Parameters)
                if (DbParam.Direction != ParameterDirection.ReturnValue)
                    Params.Add(Regex.Replace(DbParam.ParameterName, "[@:?]", ""), DbParam);

            return Params;
        }


        /// <summary>
        /// Deletes record from the table using the supplied SQL statement
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The delete statement
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the number of records deleted
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ExecuteDelete("delete from products where discontinued = 1")
        /// Db.Close();		
        /// </code>
        /// </example>
        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteDelete(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteDelete(Sql, new ListDictionary());
        }

        /// <summary>
        /// Deletes record from the table using the supplied CommandConfig
        /// </summary>
        /// <param name="CmdConfig">
        ///     <para>
        ///        The parameterised delete statement/table name and parameter values
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the number of records deleted
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// CommandConfig CmdConfig = new CommandConfig("products");
        /// // You only need to specify the table name. DbNetData will build the rest of 
        /// // the delete statement automatically using the parameters to build the where
        /// // clause
        /// CmdConfig.Params["discontinued"] = 1;
        ///
        /// Db.ExecuteDelete(CmdConfig)
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteDelete(CommandConfig CmdConfig)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteDelete(CmdConfig.Sql, CmdConfig.Params);
        }

        /// <summary>
        /// Deletes record from the table using the supplied parameterised statement and parameter values	
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The parameterised delete statement or table name
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        The paramater values
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the number of records deleted
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Params = new ListDisctionary();
        /// Params["discontinued"] = 1;
        ///
        /// Db.ExecuteDelete("products", Params)
        /// // You only need to specify the table name. DbNetData will build the rest of 
        /// // the delete statement automatically using the parameters to build the where
        /// // clause					
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteDelete(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Sql.ToLower().IndexOf("delete ") != 0)
                Sql = BuildDeleteStatement(Sql, Params);

            this.RowsAffected = ExecuteNonQuery(Sql, Params);
            return this.RowsAffected;
        }

        /// <summary>
        /// Inserts a record into the database using the supplied insert statement
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The insert statement
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the value assigned to the auto-incrementing (where applicable) column or -1
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ReturnAutoIncrementValue = true;
        /// long ProductID = Db.ExecuteInsert("insert into products (productname) values (" + ProductName.Value + ")");
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteInsert(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteInsert(Sql, new ListDictionary());
        }

        /// <summary>
        /// Inserts a record into the database using the supplied CommandConfig
        /// </summary>
        /// <param name="CmdConfig">
        ///     <para>
        ///        The parameterised insert statement/table name and parameter values
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the value assigned to the auto-incrementing (where applicable) column or -1
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// CommandConfig CmdConfig = new CommandConfig("products");
        /// // You only need to specify the table name. DbNetData will build the rest of 
        /// // the insert statement automatically using the parameters 
        /// CmdConfig.Params["productname"] = ProductName.Value;
        /// CmdConfig.Params["categoryid"] = CategoryID.Value;
        /// CmdConfig.Params["description"] = ProductName.Description;
        /// CmdConfig.Params["discontinued"] = 0;
        /// Db.ReturnAutoIncrementValue = true;
        /// long ProductID = Db.ExecuteInsert(CmdConfig)
        /// Db.Close();		
        /// </code>
        /// </example>


        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteInsert(CommandConfig CmdConfig)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteInsert(CmdConfig.Sql, CmdConfig.Params);
        }

        /// <summary>
        /// Inserts a record into the database using the supplied table name and parameters
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The insert statement or table name
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Collection of parameter values
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the value assigned to the auto-incrementing (where applicable) column or -1. The property
        ///     <see cref="DbNetLink.Data.ReturnAutoIncrementValue"/> must be set to true for the auto-incrementing value to be returned.
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDictionary Params = new ListDictionary();
        /// // the insert statement automatically using the parameters 
        /// Params["productname"] = ProductName.Value;
        /// Params["categoryid"] = CategoryID.Value;
        /// Params["description"] = ProductName.Description;
        /// Params["discontinued"] = 0;
        /// Db.ReturnAutoIncrementValue = true;
        /// long ProductID = Db.ExecuteInsert("products", Params)
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteInsert(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            Identity = -1;

            if ((this.UpdateBatchSize != 1) && !ReturnAutoIncrementValue)
            {
                UpdateInsertsTable(Sql, Params);
                return Identity;
            }

            if (Sql.ToLower().IndexOf("insert ") != 0)
                Sql = BuildInsertStatement(Sql, Params);

            if (this.Database == DatabaseType.SqlServer && ReturnAutoIncrementValue)
            {
                Sql += ";select scope_identity();";
                object Id = ExecuteScalar(Sql, Params);

                if (Id is System.Decimal)
                    Identity = Int64.Parse(Id.ToString());
            }
            else
            {
                ExecuteNonQuery(Sql, Params);

                if (ReturnAutoIncrementValue && this.Database != DatabaseType.SqlServer)
                    Identity = GetAutoIncrementValue();
            }

            return Identity;
        }


        /// <summary>
        /// Executes and ad-hoc SQL statement that does not return a record set.
        /// </summary>
        /// <param name="CmdConfig">
        ///     <para>
        ///        The SQL statement and parameters
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// CommandConfig CmdConfig = new CommandConfig("drop table products");
        /// Db.ExecuteNonQuery(CmdConfig)
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public int ExecuteNonQuery(CommandConfig CmdConfig)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteNonQuery(CmdConfig.Sql, CmdConfig.Params, false);
        }

        /// <summary>
        /// Executes and ad-hoc SQL statement that does not return a record set.
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The insert statement or table name
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ExecuteNonQuery("drop table products")
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public int ExecuteNonQuery(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteNonQuery(Sql, new ListDictionary(), false);
        }

        /// <summary>
        /// Executes and ad-hoc SQL statement that does not return a record set.
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The insert statement or table name
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Collection of parameter values
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDictionary Params = new ListDictionary();
        /// Params["param1"] = 1;
        /// Params["param2"] = CategoryID.Value;
        /// Db.ExecuteNonQuery("update products set discontinued = @param1 where category = @param2")
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public int ExecuteNonQuery(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteNonQuery(Sql, Params, false);
        }

        /// <summary>
        /// Executes and ad-hoc SQL statement that does not return a record set.
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The insert statement or table name
        ///     </para>
        /// </param>
        /// <param name="IgnoreErrors">
        ///     <para>
        ///        Suppress any encountered errors
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ExecuteNonQuery("drop table products", true)
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public int ExecuteNonQuery(string Sql, bool IgnoreErrors)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteNonQuery(Sql, new ListDictionary(), IgnoreErrors);
        }


        /// <summary>
        /// Executes and ad-hoc SQL statement that does not return a record set.
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The insert statement or table name
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Parameter values collection
        ///     </para>
        /// </param>
        /// <param name="IgnoreErrors">
        ///     <para>
        ///        Suppress any encountered errors
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ExecuteNonQuery("drop table products", null, true)
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public int ExecuteNonQuery(string Sql, IDictionary Params, bool IgnoreErrors)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Regex.Match(Sql, "^(delete|update) ", RegexOptions.IgnoreCase).Success)
                if (!Regex.Match(Sql, " where ", RegexOptions.IgnoreCase).Success)
                    if (!this.AllowUnqualifiedUpdates)
                        throw new Exception("Unqualified updates and deletes are suppressed by default. Specify 'where 1=1' or set AllowUnqualifiedUpdates to true");

            ConfigureCommand(Sql, Params);
            int RetVal = 0;

            try
            {
                RetVal = Command.ExecuteNonQuery();
            }
            catch (Exception Ex)
            {
                if (!IgnoreErrors)
                    HandleError(Ex);
            }

            WriteTrace();

            return RetVal;
        }

        /// <summary>
        /// Executes a stored procedure that returns a scalar value.
        /// </summary>
        /// <param name="Config">
        ///     <para>
        ///        Stored procedure name and parameter values collection
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the scalar value
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// CommandConfig SpConfig = new CommandConfig("sp_process_message");
        /// SpConfig.Params["message_id"] = MessageId;
        /// string StatusMessage = Db.ExecuteScalar(SpConfig).ToString()
        /// Db.Close();	
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public object ExecuteScalar(CommandConfig Config)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteScalar(Config.Sql, Config.Params);
        }

        ////////////////////////////////////////////////////////////////////////////
        public void RunScript(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            Regex RE = new Regex("^GO\\s$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            string[] Statements = RE.Split(Sql.ToString());

            CloseReader();

            foreach (string Stmnt in Statements)
            {
                if (Stmnt == "")
                    continue;

                Command.CommandText = Stmnt;

                try
                {
                    Command.ExecuteNonQuery();
                }
                catch (Exception Ex)
                {
                    if (Stmnt.ToLower().Contains("drop index"))
                        continue;
                    if (Stmnt.ToLower().Contains("add constraint"))
                        continue;

                    HandleError(Ex);
                }
            }

        }

        /// <summary>
        /// Executes a select statement or stored procedure that returns a record set. Uses the CommandConfig argument to encapsulate the
        /// command text and any parameter values.
        /// </summary>
        /// <param name="CmdConfig">
        ///     <para>
        ///        The SQL statement and parameters
        ///     </para>
        /// </param>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// CommandConfig CmdConfig = new CommandConfig("select * products");
        /// CmdConfig.Params["discontinued"] = 1;
        /// Db.ExecuteQuery(CmdConfig)
        /// DiscontinuedProductsGridView.DataSource = Db.Reader;   
        /// DiscontinuedProductsGridView.DataBind(); 
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public void ExecuteQuery(CommandConfig CmdConfig)
        ////////////////////////////////////////////////////////////////////////////
        {
            ExecuteQuery(CmdConfig.Sql, CmdConfig.Params);
        }

        /// <summary>
        /// Executes a select statement or stored procedure that returns a record set. Uses the CommandConfig argument to encapsulate the
        /// command text, parameter values and command behavior.
        /// </summary>
        /// <param name="CmdConfig">
        ///     <para>
        ///        The SQL statement, parameters and command behavior
        ///     </para>
        /// </param>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// QueryCommandConfig CmdConfig = new QueryCommandConfig("select * products");
        /// CmdConfig.Params["discontinued"] = 1;
        /// CmdConfig.Behavior = CommandBehavior.SequentialAccess;
        /// Db.ExecuteQuery(CmdConfig)
        /// DiscontinuedProductsGridView.DataSource = Db.Reader;   
        /// DiscontinuedProductsGridView.DataBind(); 
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public void ExecuteQuery(QueryCommandConfig CmdConfig)
        ////////////////////////////////////////////////////////////////////////////
        {
            ExecuteQuery(CmdConfig.Sql, CmdConfig.Params, CmdConfig.Behavior);
        }

        /// <summary>
        /// Executes a select statement or stored procedure
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ExecuteQuery("select * from products where discontinued &lt;&gt; 0");
        /// DiscontinuedProductsGridView.DataSource = Db.Reader;   
        /// DiscontinuedProductsGridView.DataBind(); 
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public void ExecuteQuery(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            ExecuteQuery(Sql, new ListDictionary(), CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a parameterised select statement or stored procedure 
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Parameter values collection
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDicitonary Params = new ListDicitonary();
        /// Params["discontinued"] = 0;
        /// Db.ExecuteQuery("select * from products where discontinued &lt;&gt; @discontinued");
        /// DiscontinuedProductsGridView.DataSource = Db.Reader;   
        /// DiscontinuedProductsGridView.DataBind(); 
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public void ExecuteQuery(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            ExecuteQuery(Sql, Params, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a parameterised select statement or stored procedure with a specified command behavior
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Parameter values collection
        ///     </para>
        /// </param>
        /// <param name="Behaviour">
        ///     <para>
        ///        CommandBehavior
        ///     </para>
        /// </param>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDicitonary Params = new ListDicitonary();
        /// Params["discontinued"] = 0;
        /// Db.ExecuteQuery("select * from products where discontinued &lt;&gt; @discontinued")
        /// DiscontinuedProductsGridView.DataSource = Db.Reader;   
        /// DiscontinuedProductsGridView.DataBind(); 
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public void ExecuteQuery(string Sql, IDictionary Params, CommandBehavior Behaviour)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Sql.ToLower().IndexOf("select ") == 0)
                if (Sql.ToLower().IndexOf(" where ") == -1 && Params.Count > 0)
                    Sql = AddWhereClause(Sql, Params);

            ConfigureCommand(Sql, Params);

            try
            {
                _Reader = Command.ExecuteReader(Behaviour);
            }
            catch (Exception Ex)
            {
                HandleError(Ex);
            }

            WriteTrace();
        }

        /// <summary>
        /// Executes a select statement or stored procedure and reads the first row of the returned record set
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /// <returns>
        /// Returns True if a record is found otherwise False
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// if ( Db.ExecuteSingletonQuery("select productname from products where id = 1") )
        ///		ProductName.Text = Db.ReaderValue("productname"); 
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public bool ExecuteSingletonQuery(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteSingletonQuery(Sql, new ListDictionary(), CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a select statement or stored procedure and reads the first row of the returned record set
        /// </summary>
        /// <param name="CmdConfig">
        ///     <para>
        ///        The SQL statement and parameter values
        ///     </para>
        /// </param>
        /// <returns>
        /// Returns True if a record is found otherwise False
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// CommandConfig CmdConfig = new CommandConfig("select productname from products");
        /// CmdConfig.Params["id"] = ProductID.Value;
        /// if ( Db.ExecuteSingletonQuery(CmdConfig) )
        ///		ProductName.Text = Db.ReaderValue("productname"); 
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public bool ExecuteSingletonQuery(QueryCommandConfig Query)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteSingletonQuery(Query.Sql, Query.Params, Query.Behavior);
        }

        /// <summary>
        /// Executes a select statement or stored procedure and reads the first row of the returned record set
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Parameter values collection
        ///     </para>
        /// </param>
        /// <returns>
        /// Returns True if a record is found otherwise False
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDictionary Params = new ListDictionary();
        /// CmdConfig.Params["id"] = ProductID.Value;
        /// if ( Db.ExecuteSingletonQuery("select productname from products", Params) )
        ///		ProductName.Text = Db.ReaderValue("productname"); 
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public bool ExecuteSingletonQuery(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteSingletonQuery(Sql, Params, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a select statement or stored procedure and reads the first row of the returned record set
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Parameter values collection
        ///     </para>
        /// </param>
        /// <param name="Behavior">
        ///     <para>
        ///        CommandBehavior enum
        ///     </para>
        /// </param> 
        /// <returns>
        /// Returns True if a record is found otherwise False
        /// </returns>


        ////////////////////////////////////////////////////////////////////////////
        public bool ExecuteSingletonQuery(string Sql, IDictionary Params, CommandBehavior Behavior)
        ////////////////////////////////////////////////////////////////////////////
        {
            ExecuteQuery(Sql, Params, Behavior);
            return _Reader.Read();
        }

        /// <summary>
        /// Executes an SQL update statement built using the supplied UpdateCommandConfig object
        /// </summary>
        /// <param name="CmdConfig">
        ///     <para>
        ///        The SQL statement, update and filter parameter collections
        ///     </para>
        /// </param>
        /// <returns>
        /// Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// UpdateCommandConfig CmdConfig = new UpdateCommandConfig("products");
        /// // It is only necessary to supply the table name as the full update statement is built automatically
        /// CmdConfig.Params["productname"] = ProductName.Value;
        /// CmdConfig.FilterParams["productid"] = ProductID.Value;
        /// Db.ExecuteUpdate(CmdConfig);
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteUpdate(UpdateCommandConfig CmdConfig)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteUpdate(CmdConfig.Sql, CmdConfig.Params, CmdConfig.FilterParams);
        }

        /// <summary>
        /// Executes an SQL update statement
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL update statement
        ///     </para>
        /// </param>
        /// <returns>
        /// Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ExecuteUpdate("update product set productname = '" + ProductName.Value + "' where productid = " + ProductID.Value );
        /// Db.Close();		
        /// </code>
        /// </example>


        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteUpdate(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteUpdate(Sql, new ListDictionary(), new ListDictionary());
        }

        /// <summary>
        /// Executes update statement built using the supplied SQL statement and parameters
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement, update and filter parameter collections
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Parameter values collection
        ///     </para>
        /// </param>
        /// <returns>
        /// Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDictionary Params = new ListDictionary();
        /// Params["productname"] = ProductName.Value;
        /// Params["productid"] = ProductID.Value;
        /// Db.ExecuteUpdate("update product set productname = @productname where productid = @productid", Params);
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteUpdate(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            return ExecuteUpdate(Sql, Params, new ListDictionary());
        }

        /// <summary>
        /// Executes update statement built using the supplied SQL statement and seperate update value and filter parameters
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement, update and filter parameter collections
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Column value parameters collection
        ///     </para>
        /// </param>
        /// <param name="FilterParams">
        ///     <para>
        ///        Filter value parameters collection
        ///     </para>
        /// </param>
        /// <returns>
        /// Returns the number of rows affected
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDictionary Params = new ListDictionary();
        /// ListDictionary FilterParams = new ListDictionary();
        /// Params["productname"] = ProductName.Value;
        /// FilterParams["productid"] = ProductID.Value;
        /// Db.ExecuteUpdate("product", Params, FilterParams);
        /// // When using separate column value and filter parameters it is only 
        /// // necessary to specify the table name in statement text
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public long ExecuteUpdate(string Sql, IDictionary Params, IDictionary FilterParams)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Sql.ToLower().IndexOf("update ") != 0)
                Sql = BuildUpdateStatement(Sql, Params, FilterParams);

            ListDictionary CombinedParams = new ListDictionary();

            foreach (string Key in Params.Keys)
                CombinedParams.Add(Key, Params[Key]);

            foreach (string Key in FilterParams.Keys)
                CombinedParams.Add(Key, FilterParams[Key]);

            this.RowsAffected = ExecuteNonQuery(Sql, CombinedParams);
            return this.RowsAffected;
        }

        /// <summary>
        /// Returns a DataSet for the supplied SQL statement
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement, update and filter parameter collections
        ///     </para>
        /// </param>
        /// <returns>
        /// A DataSet
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// DataGrid1.DataSource = Db.GetDataSet("select * from products");
        /// DataGrid1.DataBind();
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public DataSet GetDataSet(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            return GetDataSet(Sql, new ListDictionary());
        }

        /// <summary>
        /// Returns a DataSet for the supplied QueryCommandConfig
        /// </summary>
        /// <param name="CmdConfig">
        ///     <para>
        ///        Combined parameterised sql statement/procedure and parameter values
        ///     </para>
        /// </param>
        /// <returns>
        /// A DataSet
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from products");
        /// CmdConfig.Params["categoryid"] = CategoryID.Value;
        /// DataGrid1.DataSource = Db.GetDataSet(CmdConfig);
        /// DataGrid1.DataBind();
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public DataSet GetDataSet(QueryCommandConfig CmdConfig)
        ////////////////////////////////////////////////////////////////////////////
        {
            return GetDataSet(CmdConfig.Sql, CmdConfig.Params);
        }

        /// <summary>
        /// Returns a DataSet for the supplied SQL statement and parameter values
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        SQL statement or stored procedure name
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Parameter values collection
        ///     </para>
        /// </param>
        /// <returns>
        /// A DataSet
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationSettings.AppSettings["nwind"] );
        /// Db.Open();
        /// ListDictionary Params = new ListDictionary();
        /// Params["categoryid"] = CategoryID.Value;
        /// DataGrid1.DataSource = Db.GetDataSet("select * from products", Params);
        /// DataGrid1.DataBind();
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public DataSet GetDataSet(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            ConfigureCommand(Sql, Params);
            DataSet DS = new DataSet();

            try
            {
                Adapter.Fill(DS);
                if (Conn.State != ConnectionState.Open)
                    Conn.Open();
            }
            catch (Exception Ex)
            {
                HandleError(Ex);
            }
            WriteTrace();
            return DS;
        }

        /// <summary>
        /// Returns a DataTable for the supplied SQL statement
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        The SQL statement, update and filter parameter collections
        ///     </para>
        /// </param>
        /// <returns>
        /// A DataTable
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Product.DataTextField="ProductName";
        /// Product.DataValueField="ProductID";
        /// Product.DataSource = Db.GetDataTable( "select * from products" );
        /// Product.DataBind()
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public DataTable GetDataTable(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            return GetDataTable(Sql, new ListDictionary());
        }

        /// <summary>
        /// Returns a DataTable for the supplied QueryCommandConfig
        /// </summary>
        /// <param name="CmdConfig">
        ///     <para>
        ///        Combined parameterised sql statement/procedure and parameter values
        ///     </para>
        /// </param>
        /// <returns>
        /// A DataTable
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Product.DataTextField="ProductName";
        /// Product.DataValueField="ProductID";
        /// QueryCommandConfig CmdConfig = new QueryCommandConfig("select * from products where categoryid = @categoryid");
        /// CmdConfig.Params["categoryid"] = CategoryID.Value;
        /// Product.DataSource = Db.GetDataTable(CmdConfig);
        /// Product.DataBind()
        /// Db.Close();		
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public DataTable GetDataTable(QueryCommandConfig CmdConfig)
        ////////////////////////////////////////////////////////////////////////////
        {
            return GetDataSet(CmdConfig).Tables[0];
        }

        /// <summary>
        /// Returns a DataTable for the supplied SQL statement and parameter values
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        SQL statement or stored procedure name
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Parameter values collection
        ///     </para>
        /// </param>
        /// <returns>
        /// A DataTable
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Product.DataTextField="ProductName";
        /// Product.DataValueField="ProductID";
        /// ListDictionary Params = new ListDictionary();
        /// Params["categoryid"] = CategoryID.Value;
        /// Product.DataSource = Db.GetDataSet("select * from products where categoryid = @categoryid", Params);
        /// Product.DataBind()
        /// Db.Close();		
        /// </code>
        /// </example>


        ////////////////////////////////////////////////////////////////////////////
        public DataTable GetDataTable(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            return GetDataSet(Sql, Params).Tables[0];
        }

        /// <summary>
        /// Returns the data for the current record in the data reader as a Hashtable
        /// </summary>
        /// <returns>
        /// A Hashtable
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDictionary Params = new ListDictionary();
        /// Params["productid"] = ProductID.Value;
        /// Hashtable ProductData = new Hashtable();
        /// if ( Db.ExecuteSingletonQuery("select * from products where productid = @productid", Params) )
        ///     ProductData = Db.GetHashtable();
        /// Db.Close();		
        /// </code>
        /// </example>


        ////////////////////////////////////////////////////////////////////////////
        public Hashtable GetHashtable(QueryCommandConfig Query)
        ////////////////////////////////////////////////////////////////////////////
        {
            ExecuteSingletonQuery(Query);
            return GetHashtable();
        }

        ////////////////////////////////////////////////////////////////////////////
        public Hashtable GetHashtable()
        ////////////////////////////////////////////////////////////////////////////
        {
            Hashtable Data = new Hashtable(StringComparer.OrdinalIgnoreCase);

            if (this._Reader.IsClosed)
                return Data;

            try
            {
                for (int i = 0; i < this._Reader.FieldCount; i++)
                    Data[this._Reader.GetName(i)] = this.ReaderValue(i);
            }
            catch (Exception)
            {
            }
            _Reader.Close();
            return Data;
        }

        /// <summary>
        /// Returns a DataTable containing column metadata for the supplied SQL statement/table name
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        SQL statement or table name
        ///     </para>
        /// </param>
        /// <returns>
        /// A DataTable
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// DataTable SchemaInfo = Db.GetSchemaTable("customers");
        /// foreach ( DataRow R in 	SchemaInfo.Rows )
        ///		if (R["ColumnName"].ToString() == "CustomID" )
        ///			if ( (bool)R["IsKey"]  )
        ///				CustomerID.ReadOnly = true;
        /// </code>
        /// </example>

        ////////////////////////////////////////////////////////////////////////////
        public DataTable GetSchemaTable(string Sql)
        ////////////////////////////////////////////////////////////////////////////
        {
            return GetSchemaTable(Sql, new ListDictionary());
        }

        /// <summary>
        /// Returns a DataTable containing column metadata for the supplied SQL statement/table name and parameters
        /// </summary>
        /// <param name="Sql">
        ///     <para>
        ///        SQL statement or table name
        ///     </para>
        /// </param>
        /// <param name="Params">
        ///     <para>
        ///        Parameter values collection
        ///     </para>
        /// </param>
        /// <returns>
        /// A DataTable
        /// </returns>

        ////////////////////////////////////////////////////////////////////////////
        public DataTable GetSchemaTable(string Sql, ListDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            string[] TableName = new string[0];
            if (Sql.ToLower().IndexOf("select ") != 0)
            {
                TableName = Sql.Split('.');
                Sql = "select * from " + this.QualifiedDbObjectName(Sql) + " where 1=2";
            }

            ExecuteQuery(Sql, Params, CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
            DataTable T = _Reader.GetSchemaTable();

            if (!T.Columns.Contains("DataTypeName"))
                T.Columns.Add(new DataColumn("DataTypeName", System.Type.GetType("System.String")));

            T.Columns.Add(new DataColumn("FieldTypeName", System.Type.GetType("System.String")));
            T.Columns.Add(new DataColumn("ProviderFieldTypeName", System.Type.GetType("System.String")));

            T.Columns["DataTypeName"].ReadOnly = false;

            for (int I = 0; I < _Reader.FieldCount; I++)
            {
                T.Rows[I]["DataTypeName"] = _Reader.GetDataTypeName(I);
                T.Rows[I]["FieldTypeName"] = _Reader.GetFieldType(I).ToString();

                switch (_Reader.GetType().Name)
                {
                    case "SqlDataReader":
                    case "SqlCeDataReader":
                        Object[] Args = new Object[1];
                        Args[0] = I;
                        T.Rows[I]["ProviderFieldTypeName"] = InvokeMethod(_Reader, "GetProviderSpecificFieldType", Args).ToString();
                        break;
                }

                //if (Reader is SqlDataReader)
                //    T.Rows[I]["ProviderFieldTypeName"] = ((SqlDataReader)Reader).GetProviderSpecificFieldType(I).ToString();
            }

            _Reader.Close();

            if (this.Database == DatabaseType.SqlServer && TableName.Length > 0)
            {
                Sql = "SELECT K.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE K ON T.CONSTRAINT_NAME = K.CONSTRAINT_NAME WHERE T.CONSTRAINT_TYPE = 'PRIMARY KEY' AND T.TABLE_NAME = '" + TableName[TableName.Length - 1] + "'";

                if (TableName.Length == 2)
                {
                    Sql += " AND T.TABLE_SCHEMA = '" + TableName[0] + "'";
                }

                ExecuteQuery(Sql);
                while (_Reader.Read())
                {
                    if (T.Columns["IsKey"].ReadOnly)
                    {
                        T.Columns["IsKey"].ReadOnly = false;
                        foreach (DataRow R in T.Rows)
                            R["IsKey"] = false;
                    }
                    DataRow[] Rows = T.Select("ColumnName = '" + _Reader.GetString(0) + "'");
                    Rows[0]["IsKey"] = true;
                }
                _Reader.Close();
            }

            switch (this.Database)
            {
                case DatabaseType.MySql:
                    T.Columns["ColumnSize"].ReadOnly = false;

                    foreach (DataRow Row in T.Rows)
                    {
                        if (Row["ColumnSize"].ToString() != "-1")
                            continue;

                        switch (Row["ProviderType"].ToString())
                        {
                            case "749":
                            case "750":
                            case "751":
                            case "752":
                                Row["ColumnSize"] = System.Int32.MaxValue;
                                break;
                        }
                    }
                    break;
                case DatabaseType.Pervasive:
                    T.Columns["ColumnSize"].ReadOnly = false;

                    foreach (DataRow Row in T.Rows)
                    {

                        switch (Row["ProviderType"].ToString())
                        {
                            case "20":
                                Row["ColumnSize"] = 2;
                                break;
                        }
                    }
                    break;

                case DatabaseType.Firebird:
                    T.Columns["NumericPrecision"].ReadOnly = false;
                    T.Columns["NumericScale"].ReadOnly = false;

                    foreach (DataRow Row in T.Rows)
                    {
                        if (Row["NumericPrecision"] == System.DBNull.Value)
                            Row["NumericPrecision"] = 0;
                        if (Row["NumericScale"] == System.DBNull.Value)
                            Row["NumericScale"] = 0;
                    }
                    break;
                case DatabaseType.PostgreSql:
                    T.Columns["ColumnSize"].ReadOnly = false;

                    foreach (DataRow Row in T.Rows)
                    {
                        if (Row["ColumnSize"].ToString() == "-1")
                            Row["ColumnSize"] = System.Int32.MaxValue;
                    }
                    break;
            }

            return T;
        }

        /// <summary>
        /// Returns the next or current Oracle sequence value
        /// </summary>
        /// <param name="SequenceName">
        ///     <para>
        ///        The name of the Oracle Sequence
        ///     </para>
        /// </param>
        /// <param name="Increment">
        ///     <para>
        ///        If true returns the next value otherwise returns the current value
        ///     </para>
        /// </param>
        /// <returns>
        /// Boolean
        /// </returns>

        ////////////////////////////////////////////////////////////////////////////
        public Int64 GetSequenceValue(string SequenceName, bool Increment)
        ////////////////////////////////////////////////////////////////////////////
        {

            switch (this.Database)
            {
                case DatabaseType.Oracle:
                    break;
                default:
                    throw new Exception("GetSequenceValue not supported for this database");
                    break;
            }

            string Sql = "select " + SequenceName + "." + ((Increment) ? "next" : "curr") + "val from dual";
            this.ExecuteSingletonQuery(Sql);
            return Convert.ToInt64(_Reader.GetValue(0));

        }

        /// <summary>
        /// Returns true if the Token parameter is a reserved word in the database
        /// </summary>
        /// <param name="Token">
        ///     <para>
        ///        The reserved word
        ///     </para>
        /// </param>
        /// <returns>
        /// Boolean
        /// </returns>

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool IsReservedWord(string Token)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            return (ReservedWords[Token.ToUpper()] != null);
        }

        /// <summary>
        /// Returns a ListDictionary object representing the current record in a format suitable for serialising in JSON format
        /// </summary>
        /// <returns>
        /// ListDictionary
        /// </returns>


        ///////////////////////////////////////////////
        public ListDictionary JsonRecord()
        ///////////////////////////////////////////////
        {
            ListDictionary Data = new ListDictionary();

            if (_Reader != null)
                if (!_Reader.IsClosed)
                    for (int i = 0; i < _Reader.FieldCount; i++)
                        Data[_Reader.GetName(i).ToLower()] = JsonValue(i);

            return Data;
        }

        /// <summary>
        /// Returns a DataTable containing database metadata of the specified type
        /// </summary>
        /// <param name="CollectionType">
        ///     <para>
        ///        The Meta data collection type
        ///     </para>
        /// </param>
        /// <returns>
        /// DataTable
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// DatabaseTables.DataTextField="TableName";
        /// DatabaseTables.DataValueField="TableName";
        /// DatabaseTables.DataSource = Db.MetaDataCollection( MetaDataType.Tables );
        /// DatabaseTables.DataBind();
        /// Db.Close();		
        /// </code>
        /// </example>


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public DataTable MetaDataCollection(MetaDataType CollectionType)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string GetSchemaArg = CollectionType.ToString();

            switch (CollectionType)
            {
                case MetaDataType.UserTables:
                    GetSchemaArg = "Tables";
                    break;
                case MetaDataType.UserViews:
                    GetSchemaArg = "Views";
                    break;
                case MetaDataType.Functions:
                    GetSchemaArg = "Procedures";
                    break;
            }

            Object T = new Object();

            /*
            if (Provider == DataProvider.OleDb && Database == DatabaseType.SqlServer)
            {

                switch (CollectionType)
                {
                    case MetaDataType.Tables:
                    case MetaDataType.Views:
                    case MetaDataType.UserTables:
                    case MetaDataType.UserViews:
                        T = this.GetDataTable("select * from information_schema." + GetSchemaArg);
                        break;
                }
            }
             */

            if (!(T is DataTable))
            {
                Object[] Args = new Object[1];
                Args[0] = GetSchemaArg;

                T = InvokeMethod(Conn, "GetSchema", Args);
            }

            if (T is DataTable)
            {
                DataTable Schema = (DataTable)T;
                switch (CollectionType)
                {
                    case MetaDataType.DataTypes:
                        RemapDataTypesSchemaColumnNames(Schema);
                        break;
                    case MetaDataType.Tables:
                    case MetaDataType.Views:
                        RemapTablesSchemaColumnNames(Schema);
                        break;
                    case MetaDataType.UserTables:
                    case MetaDataType.UserViews:
                        RemapTablesSchemaColumnNames(Schema);
                        DataRow[] Rows = Schema.Select(CollectionType == MetaDataType.UserTables ? UserTableFilter() : UserViewFilter());
                        DataTable UserTables = Schema.Clone();
                        foreach (DataRow R in Rows)
                            UserTables.ImportRow(R);
                        Schema = UserTables;
                        break;
                    case MetaDataType.Procedures:
                    case MetaDataType.Functions:
                        switch (Database)
                        {
                            case DatabaseType.SqlServer:
                                DataRow[] PRows = Schema.Select("ROUTINE_TYPE = '" + CollectionType.ToString().ToUpper().Replace("S", "") + "'");
                                DataTable PT = Schema.Clone();
                                foreach (DataRow R in PRows)
                                    PT.ImportRow(R);
                                Schema = PT;
                                break;
                        }
                        break;
                }

                return Schema;
            }
            else
            {
                switch (this.Provider)
                {
                    case DataProvider.Npgsql:
                        return NpgSqlSchemaInfo(CollectionType);
                    case DataProvider.SqlServerCE:
                        return SqlServerCESchemaInfo(CollectionType);
                    default:
                        return new DataTable();
                }
            }
        }

        /// <summary>
        /// Returns a DataTable containing database metadata of the specified type
        /// </summary>
        /// <param name="CollectionType">
        ///     <para>
        ///        The Meta data collection type
        ///     </para>
        /// </param>
        /// <returns>
        /// DataTable
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// DatabaseTables.DataTextField="TableName";
        /// DatabaseTables.DataValueField="TableName";
        /// DatabaseTables.DataSource = Db.MetaDataCollection( "Tables" );
        /// DatabaseTables.DataBind();
        /// Db.Close();		
        /// </code>
        /// </example> 

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public DataTable MetaDataCollection(string CollectionType)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            MetaDataType MDT;

            try
            {
                MDT = (MetaDataType)Enum.Parse(typeof(MetaDataType), CollectionType, true);
            }
            catch
            {
                throw new Exception("Collection type ==> " + CollectionType + " is not valid");
            }

            return MetaDataCollection(MDT);
        }

        /// <summary>
        /// Opens the database connection
        /// </summary>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ...
        /// Db.Close();		
        /// </code>
        /// </example> 

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Open(string ConnectionString, DataProvider? Provider, DatabaseType Database = DatabaseType.Unknown)
        {
            this._ConnectionString = MapDatabasePath(ConnectionString);
            this._Provider = Provider.HasValue ? Provider.Value : DeriveProvider(this._ConnectionString);
            this._Database = Database;
            try
            {
                switch (Provider)
                {
                    case DataProvider.OleDb:
                        Conn = new OleDbConnection(this.ConnectionString);
                        Adapter = new OleDbDataAdapter();
                        ProviderAssembly = Assembly.GetAssembly(typeof(System.Data.OleDb.OleDbConnection));
                        break;
                    case DataProvider.Odbc:
                        Conn = new OdbcConnection(this.ConnectionString);
                        Adapter = new OdbcDataAdapter();
                        ProviderAssembly = Assembly.GetAssembly(typeof(System.Data.Odbc.OdbcConnection));
                        break;
                    case DataProvider.SqlClient:
                        Conn = new SqlConnection();
                        Adapter = new SqlDataAdapter();
                        this._Database = DatabaseType.SqlServer;
                     //   ProviderAssembly = Assembly.GetAssembly(typeof(System.Data.SqlClient.SqlConnection));
                        break;
                    default:
                        GetCustomProviderConnection();
                        break;

                }
            }
            catch (Exception Ex)
            {
                HandleError(Ex);
            }

            try
            {
                SetPropertyValue(Adapter, "UpdateBatchSize", 10);
                this._BatchUpdateSupported = true;
                SetPropertyValue(Adapter, "UpdateBatchSize", 1);
            }
            catch (Exception)
            {
            }

            Command = Conn.CreateCommand();

            if (this.Provider == DataProvider.Oracle)
                SetPropertyValue(Command, "BindByName", true);

            Adapter.SelectCommand = Command;
            if (this.Provider == DataProvider.SqlServerCE)
                CheckSQLCEVersion();

            try
            {
                Conn.Open();
            }
            catch (Exception Ex)
            {
                HandleError(Ex);
            }

            switch (Provider)
            {
                case DataProvider.OleDb:
                case DataProvider.Odbc:
                    if (this.Database == DatabaseType.Unknown)
                        this._Database = GetDatabaseType();
                    ParameterTemplate = "?";
                    break;
            }

            switch (this.Database)
            {
                case DatabaseType.SqlServer:
                case DatabaseType.SqlServerCE:
                case DatabaseType.Access:
                case DatabaseType.Access2007:
                case DatabaseType.Excel:
                case DatabaseType.Excel2007:
                case DatabaseType.VistaDB:
                case DatabaseType.Sybase:
                    NameDelimiterTemplate = "[{0}]";
                    break;
                case DatabaseType.PostgreSql:
                case DatabaseType.DB2:
                case DatabaseType.Oracle:
                case DatabaseType.InterSystemsCache:
                case DatabaseType.Advantage:
                case DatabaseType.Firebird:
                case DatabaseType.Progress:
                case DatabaseType.Pervasive:
                case DatabaseType.Paradox:
                case DatabaseType.SQLite:
                    NameDelimiterTemplate = "\"{0}\"";
                    break;
                case DatabaseType.MySql:
                    NameDelimiterTemplate = "`{0}`";
                    break;
                case DatabaseType.VisualFoxPro:
                    NameDelimiterTemplate = "{0}";
                    break;
            }

        }

        /// <summary>
        /// Returns then corresponding database parameter name for the supplied token
        /// </summary>
        /// <param name="Key">
        ///     <para>
        ///        The unformatted name of the parameter e.g. "userid"
        ///     </para>
        /// </param>
        /// <returns>
        /// The qualified parameter name. For example for an MS SQL Server connection the key "userid" would be 
        /// returned as "@userid".
        /// </returns>

        ////////////////////////////////////////////////////////////////////////////
        public string ParameterName(string Key)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Key.Length > 0 && ParameterTemplate.Length > 1)
                if (ParameterTemplate.Substring(0, 1) == Key.Substring(0, 1))
                    return Key;

            return ParameterTemplate.Replace("{0}", CleanParameterName(Key));
        }

        /// <summary>
        /// Qualifies a database object name to ensure that it can can be used in an SQL Statement.
        /// </summary>
        /// <remarks>
        /// The function only qualifies the name if necessary for example if the name contains an embeded space or is a reserved word.
        /// </remarks>
        /// <param name="ObjectName">
        ///     <para>
        ///        The name of the column, table, index etc
        ///     </para>
        /// </param>
        /// <returns>
        /// The qualified object name. For example for a MS SQL Server connection the name "order details" would become "[order details]".
        /// </returns>


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string QualifiedDbObjectName(string ObjectName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string[] NameParts = ObjectName.Split('.');

            switch (this.Database)
            {
                case DatabaseType.Oracle:
                    if (NameParts[NameParts.Length - 1].Length > 30)
                        NameParts[NameParts.Length - 1] = NameParts[NameParts.Length - 1].Substring(0, 30);
                    break;
                case DatabaseType.Firebird:
                    if (NameParts[NameParts.Length - 1].Length > 31)
                        NameParts[NameParts.Length - 1] = NameParts[NameParts.Length - 1].Substring(0, 31);
                    break;
                case DatabaseType.Access:
                    NameParts = ObjectName.Replace(".", "_").Split('.');
                    if (NameParts[0].Length > 64)
                        NameParts[0] = NameParts[0].Substring(0, 64);
                    break;
                case DatabaseType.MySql:
                    if (NameParts[NameParts.Length - 1].Length > 64)
                        NameParts[NameParts.Length - 1] = NameParts[NameParts.Length - 1].Substring(0, 64);
                    break;
            }

            Regex RE = new Regex(NameDelimiterTemplate.Replace("[", @"\[").Replace("]", @"\]").Replace("{0}", ".*"));

            for (int I = 0; I < NameParts.Length; I++)
                if (Regex.IsMatch(NameParts[I], @"\W") || IsReservedWord(NameParts[I]) || StartsWithNumeric(NameParts[I]) || this.Database == DatabaseType.Firebird || this.QualifyAllObjectNames)
                    if (!RE.IsMatch(NameParts[I]) || this.Database == DatabaseType.Firebird)
                        NameParts[I] = NameDelimiterTemplate.Replace("{0}", NameParts[I].Trim());

            return string.Join(".", NameParts);
        }

        /// <summary>
        /// Removes the qualifiers from a database object name.
        /// </summary>
        /// <param name="ObjectName">
        ///     <para>
        ///        The qualified name of the column, table, index etc
        ///     </para>
        /// </param>
        /// <returns>
        /// The unqualified object name. For example for a MS SQL Server connection the name "[order details]" would become "order details".
        /// </returns>

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string UnqualifiedDbObjectName(string ObjectName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            return UnqualifiedDbObjectName(ObjectName, false);
        }

        /// <summary>
        /// Removes the qualifiers from a database object name.
        /// </summary>
        /// <param name="ObjectName">
        ///     <para>
        ///        The qualified name of the column, table, index etc
        ///     </para>
        /// </param>
        /// <param name="BaseNameOnly">
        ///     <para>
        ///        Only unqualifies the base part of the database object name
        ///     </para>
        /// </param>
        /// <returns>
        /// The unqualified object name. For example for a MS SQL Server connection the name "[northwind].[order details]" would become "[northwind].order details".
        /// </returns>

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string UnqualifiedDbObjectName(string ObjectName, bool BaseNameOnly)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string[] NameParts = ObjectName.Split('.');

            Regex RE = new Regex(@"[\[\]\`""]");

            for (int I = 0; I < NameParts.Length; I++)
                NameParts[I] = RE.Replace(NameParts[I], "");

            if (BaseNameOnly)
                return NameParts[NameParts.Length - 1];
            else
                return string.Join(".", NameParts);
        }

        /// <summary>
        /// Returns the string value for the current row in the Reader for the specified column name
        /// </summary>
        /// <param name="ColumnName">
        ///     <para>
        ///        The name of the column
        ///     </para>
        /// </param>
        /// <returns>
        /// The reader column value string
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ExecuteQuery("select * from subscribers");
        /// while ( Db.Reader.Read() )
        /// {
        ///		SendMailMessage( Daa.ReaderString("email_address") )
        /// }
        /// Db.Close();		
        /// </code>
        /// </example> 


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string ReaderString(string ColumnName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            return ReaderValue(ColumnName).ToString();
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string ReaderString(int ColumnIndex)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            return ReaderValue(ColumnIndex).ToString();
        }
        /// <summary>
        /// Returns the value for the current row in the Reader for the specified column name
        /// </summary>
        /// <param name="ColumnName">
        ///     <para>
        ///        The name of the column
        ///     </para>
        /// </param>
        /// <returns>
        /// The reader column value
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ExecuteQuery("select * from subscribers");
        /// while ( Db.Reader.Read() )
        /// {
        ///		SendMailMessage( Daa.ReaderValue("email_address").ToString() )
        /// }
        /// Db.Close();		
        /// </code>
        /// </example> 
        /// 

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public object ReaderValue(string ColumnName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                return ReaderValue(_Reader.GetOrdinal(ColumnName));
            }
            catch (Exception)
            {
                throw new Exception("ReaderValue column not found: " + ColumnName);
            }

        }

        /// <summary>
        /// Returns the value for the current row in the Reader for the specified column index
        /// </summary>
        /// <param name="ColumnIndex">
        ///     <para>
        ///        The inedx of the column
        ///     </para>
        /// </param>
        /// <returns>
        /// The reader column value
        /// </returns>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// Db.ExecuteSingletonQuery("select count(*) from subscribers");
        /// SubsriberCount.Value = Db.ReaderValue(0);
        /// Db.Close();		
        /// </code>
        /// </example> 		

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public object ReaderValue(int ColumnIndex)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            return _Reader.GetValue(ColumnIndex);
        }

        /// <summary>
        /// Sets the value of a parameter in a collection irrespective of case-sensitivity and whether the parameter is a simple 
        /// value or IDbDataParameter object 
        /// </summary>
        /// <param name="Params">
        ///     <para>
        ///        The parameter collection
        ///     </para>
        /// </param>
        /// <param name="ParamName">
        ///     <para>
        ///        The name of the parameter
        ///     </para>
        /// </param>
        /// <param name="ParamValue">
        ///     <para>
        ///        The parameter value to assign
        ///     </para>
        /// </param>
        /// <example>
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// ListDictionary Params = Db.DeriveParameters("CustOrderHist");
        /// Db.SetParamValue( Params, "customerid", Customers.SelectedValue );
        /// Db.ExecuteQuery( "CustOrderHist", Params );
        /// ResultsGridView.DataSource = Db.Reader;
        /// ResultsGridView.DataBind();
        /// Db.Close();		
        /// </code>
        /// </example>	

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void SetParamValue(ListDictionary Params, string ParamName, object ParamValue)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            bool Found = false;
            foreach (string K in Params.Keys)
            {
                if (K.ToLower() == ParamName.ToLower())
                {
                    if (Params[K] is IDbDataParameter)
                        ((IDbDataParameter)Params[K]).Value = ParamValue;
                    else
                        Params[K] = ParamValue;
                    Found = true;
                    break;
                }
            }

            if (!Found)
                Params[ParamName] = ParamValue;
        }

        /// <summary>
        /// Creates a vendor independent filter string for the Tables metadata collection to select on "user" tables.
        /// </summary>
        /// <returns>
        /// The filter string
        /// </returns>
        /// <example>	
        /// <code>
        /// DbNetData Db = new DbNetData( ConfigurationManager.ConnectionStrings["nwind"] );
        /// Db.Open();
        /// DataRow[] UserTables = Db.MetaDataCollection( MetaDataType.Tables ).Select( Db.UserTableFilter() );
        /// Db.Close();		
        /// </code>
        /// </example>	


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string UserTableFilter()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string Filter = "";

            switch (this.Provider)
            {
                case DataProvider.Odbc:
                    Filter = "TABLE_TYPE = 'TABLE'";
                    break;
                default:
                    switch (this.Database)
                    {
                        case DatabaseType.Oracle:
                            Filter = "TABLE_TYPE = 'User'";
                            break;
                        case DatabaseType.PostgreSql:
                            if (this.Provider == DataProvider.Npgsql)
                                Filter = "TABLE_TYPE = 'BASE TABLE' and TABLE_SCHEMA not in ('pg_catalog','information_schema')";
                            else
                                Filter = "TABLE_TYPE = 'User'";
                            break;
                        case DatabaseType.SqlServer:
                            if (this.Provider == DataProvider.OleDb)
                                Filter = "TABLE_TYPE = 'TABLE'";
                            else
                                Filter = "TABLE_TYPE = 'BASE TABLE' and TABLE_NAME <> 'dtproperties'";
                            break;
                        case DatabaseType.MySql:
                            Filter = "TABLE_TYPE = 'BASE TABLE' and TABLE_NAME <> 'dtproperties'";
                            break;
                        case DatabaseType.DB2:
                            Filter = "TABLE_TYPE = 'TABLE' and TABLE_SCHEMA <> 'SYSTOOLS'";
                            break;
                        case DatabaseType.Excel:
                        case DatabaseType.Excel2007:
                            Filter = "TABLE_NAME like '%$' or TABLE_NAME like '%$'''";
                            break;
                        case DatabaseType.VistaDB:
                            Filter = "TABLE_TYPE in ('BASE TABLE','TABLE')";
                            break;
                        default:
                            Filter = "TABLE_TYPE = 'TABLE'";
                            break;
                    }
                    break;
            }

            return Filter;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string UserViewFilter()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string Filter = "";

            switch (this.Database)
            {
                case DatabaseType.Oracle:
                    Filter = "TABLE_SCHEMA not in ('SYS','SYSTEM')";
                    break;
            }

            return Filter;
        }
        #endregion

        #region Private Methods
        ////////////////////////////////////////////////////////////////////////////
        private string AddWhereClause(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Params.Count == 0)
                return Sql;

            return Sql + " where " + BuildParamFilter(Params);
        }

        ////////////////////////////////////////////////////////////////////////////
        internal string BuildParamFilter(IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Params.Count == 0)
                return "";

            List<string> Parameters = new List<string>();

            foreach (string Key in Params.Keys)
                Parameters.Add(QualifiedDbObjectName(Key) + " = " + ParameterName(Key));

            return "(" + string.Join(" and ", Parameters.ToArray()) + ")";
        }


        ////////////////////////////////////////////////////////////////////////////
        private string BuildUpdateStatement(string TableName, IDictionary Params, IDictionary FilterParams)
        ////////////////////////////////////////////////////////////////////////////
        {
            List<string> Parameters = new List<string>();

            foreach (string Key in Params.Keys)
                Parameters.Add(QualifiedDbObjectName(Key) + " = " + ParameterName(Key));

            string Sql = "update " + TableName + " set " + string.Join(",", Parameters.ToArray());

            return this.AddWhereClause(Sql, FilterParams);
        }

        ////////////////////////////////////////////////////////////////////////////
        private string BuildInsertStatement(string TableName, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            List<string> ColumnNames = new List<string>();
            List<string> ParameterNames = new List<string>();

            foreach (string Key in Params.Keys)
            {
                ColumnNames.Add(QualifiedDbObjectName(Key));
                ParameterNames.Add(ParameterName(Key));
            }

            return "insert into " + TableName + "(" + string.Join(",", ColumnNames.ToArray()) + ") values (" + string.Join(",", ParameterNames.ToArray()) + ")";
        }

        ////////////////////////////////////////////////////////////////////////////
        private string BuildSelectStatement(string TableName, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            List<string> ColumnNames = new List<string>();
            List<string> ParameterNames = new List<string>();

            foreach (string Key in Params.Keys)
            {
                ColumnNames.Add(QualifiedDbObjectName(Key));
                ParameterNames.Add(ParameterName(Key));
            }

            return "select " + string.Join(",", ColumnNames.ToArray()) + " from " + TableName + " where 1=2";
        }

        ////////////////////////////////////////////////////////////////////////////
        private string BuildDeleteStatement(string TableName, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            return this.AddWhereClause("delete from " + TableName, Params);
        }

        ////////////////////////////////////////////////////////////////////////////
        private void ConfigureCommand(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            CloseReader();
            Command.CommandText = Sql.Trim();

            if (Regex.Match(Command.CommandText, "^(alter|drop|create|select|insert|delete|update|truncate|set|if|begin|print) ", RegexOptions.IgnoreCase).Success)
                Command.CommandType = CommandType.Text;
            else
                Command.CommandType = CommandType.StoredProcedure;

            if (this.CommandTimeout > -1)
                if (this.Database != DatabaseType.SqlServerCE)
                    Command.CommandTimeout = this.CommandTimeout;

            Command.Parameters.Clear();
            AddCommandParameters(Params);

            if (OnCommandConfigured != null)
                OnCommandConfigured(this, Command);

            CommandStart = System.DateTime.Now;
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private DatabaseType GetDatabaseType()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            DataTable DbInfo = MetaDataCollection(MetaDataType.DataSourceInformation);

            if (DbInfo.Rows.Count == 0)
                return DatabaseType.Unknown;

            DataSourceInfo = DbInfo.Rows[0];

            string DataSourceProductName = DataSourceInfo["DataSourceProductName"].ToString().ToLower();

            if (DataSourceProductName.IndexOf("microsoft sql server") > -1)
                return DatabaseType.SqlServer;
            if (DataSourceProductName.IndexOf("mysql") > -1)
                return DatabaseType.MySql;
            if (DataSourceProductName.IndexOf("oracle") > -1)
                return DatabaseType.Oracle;
            if (DataSourceProductName.IndexOf("intersystems cache") > -1)
                return DatabaseType.InterSystemsCache;
            if (DataSourceProductName.IndexOf("ms jet") > -1)
                return DatabaseType.Access;
            if (DataSourceProductName.IndexOf("db2") > -1)
                return DatabaseType.DB2;
            if (DataSourceProductName.IndexOf("firebird") > -1)
                return DatabaseType.Firebird;
            if (DataSourceProductName.IndexOf("sybase") > -1)
                return DatabaseType.Sybase;
            if (DataSourceProductName.IndexOf("postgresql") > -1)
                return DatabaseType.PostgreSql;
            if (DataSourceProductName.IndexOf("pervasive") > -1)
                return DatabaseType.Pervasive;
            if (DataSourceProductName.IndexOf("openedge") > -1)
                return DatabaseType.Progress;
            if (DataSourceProductName.IndexOf("microsoft visual foxpro") > -1)
                return DatabaseType.VisualFoxPro;
            if (DataSourceProductName.IndexOf("paradox") > -1)
                return DatabaseType.Paradox;

            return DatabaseType.Unknown;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void GetCustomProviderConnection()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string PartialName = "";
            string ConnectionClassName = "";

            switch (Provider)
            {
                case DataProvider.Oracle:
                    PartialName = "Oracle.DataAccess";
                    ConnectionClassName = "Client.OracleConnection";
                    this._Database = DatabaseType.Oracle;
                    break;
                case DataProvider.OracleClient:
                    PartialName = "System.Data.OracleClient";
                    ConnectionClassName = "OracleConnection";
                    this._Database = DatabaseType.Oracle;
                    break;
                case DataProvider.Sybase:
                    PartialName = "Sybase.Data.AseClient";
                    ConnectionClassName = "AseConnection";
                    this._Database = DatabaseType.Sybase;
                    break;
                case DataProvider.MySql:
                    PartialName = "MySql.Data";
                    ConnectionClassName = "MySqlClient.MySqlConnection";
                    this._Database = DatabaseType.MySql;
                    break;
                case DataProvider.MyDirect:
                    PartialName = "CoreLab.MySql";
                    ConnectionClassName = "MySqlConnection";
                    this._Database = DatabaseType.MySql;
                    break;
                case DataProvider.Npgsql:
                    PartialName = "Npgsql";
                    ConnectionClassName = "NpgsqlConnection";
                    this._Database = DatabaseType.PostgreSql;
                    break;
                case DataProvider.PostgreSqlDirect:
                    PartialName = "CoreLab.PostgreSql";
                    ConnectionClassName = "PgSqlConnection";
                    this._Database = DatabaseType.PostgreSql;
                    break;
                case DataProvider.Firebird:
                    PartialName = "FirebirdSql.Data.FirebirdClient";
                    ConnectionClassName = "FbConnection";
                    this._Database = DatabaseType.Firebird;
                    break;
                case DataProvider.DB2:
                    PartialName = "IBM.Data.DB2";
                    ConnectionClassName = "DB2Connection";
                    this._Database = DatabaseType.DB2;
                    break;
                case DataProvider.Pervasive:
                    PartialName = "Pervasive.Data.SqlClient";
                    ConnectionClassName = "PsqlConnection";
                    this._Database = DatabaseType.Pervasive;
                    ParameterTemplate = "?";
                    break;
                case DataProvider.Odbc:
                    PartialName = "Microsoft.Data.Odbc";
                    ConnectionClassName = "OdbcConnection";
                    ParameterTemplate = "?";
                    break;
                case DataProvider.VistaDB:
                    PartialName = "VistaDB.4";
                    ConnectionClassName = "VistaDBConnection";
                    this._Database = DatabaseType.VistaDB;
                    break;
                case DataProvider.SybaseDataDirect:
                    PartialName = "DDTek.Sybase";
                    ConnectionClassName = "SybaseConnection";
                    this._Database = DatabaseType.Sybase;
                    break;
                case DataProvider.InterSystemsCache:
                    PartialName = "InterSystems.Data.CacheClient";
                    ConnectionClassName = "CacheConnection";
                    this._Database = DatabaseType.InterSystemsCache;
                    break;
                case DataProvider.Advantage:
                    PartialName = "Advantage.Data.Provider";
                    ConnectionClassName = "AdsConnection";
                    this._Database = DatabaseType.Advantage;
                    break;
                case DataProvider.SQLite:
                    PartialName = "System.Data.SQLite";
                    ConnectionClassName = "SQLiteConnection";
                    this._Database = DatabaseType.SQLite;
                    break;
                case DataProvider.SqlServerCE:
                    AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);
                    PartialName = "System.Data.SqlServerCe";
                    ConnectionClassName = "SqlCeConnection";
                    this._Database = DatabaseType.SqlServerCE;
                    break;
            }

            switch (this.Database)
            {
                case DatabaseType.Oracle:
                case DatabaseType.PostgreSql:
                case DatabaseType.Advantage:
                case DatabaseType.SQLite:
                    ParameterTemplate = ":{0}";
                    break;
                case DatabaseType.MySql:
                    ParameterTemplate = "?{0}";
                    break;
            }

            ProviderAssembly = Assembly.LoadWithPartialName(PartialName);

            if (ProviderAssembly == null)
            {
                switch (PartialName)
                {
                    case "FirebirdSql.Data.FirebirdClient":
                        PartialName = "FirebirdSql.Data.Firebird";
                        ProviderAssembly = Assembly.LoadWithPartialName(PartialName);
                        break;
                    case "VistaDB.4":
                        PartialName = "VistaDB.NET20";
                        ProviderAssembly = Assembly.LoadWithPartialName(PartialName);
                        break;
                }
            }

            if (ProviderAssembly == null)
            {
                switch (PartialName)
                {
                    case "VistaDB.NET20":
                        PartialName = "VistaDB.NET11";
                        ProviderAssembly = Assembly.LoadWithPartialName(PartialName);
                        break;
                }
            }

            if (ProviderAssembly == null)
                throw new Exception("GetCustomProviderConnection:Failed to load assembly --> " + PartialName + ". Please install the .Net Data provider (" + PartialName + ").");

            switch (Provider)
            {
                case DataProvider.VistaDB:
                    PartialName = "VistaDB.Provider";
                    break;
            }

            Type ConnectionType = ProviderAssembly.GetType(PartialName + "." + ConnectionClassName, true);
            Type AdapterType = ProviderAssembly.GetType(PartialName + "." + ConnectionClassName.Replace("Connection", "DataAdapter"), true);

            Object[] Args = new Object[1];
            Args[0] = this.ConnectionString;

            Conn = (IDbConnection)Activator.CreateInstance(ConnectionType, Args);
            Adapter = (IDbDataAdapter)Activator.CreateInstance(AdapterType);

        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private string LookupConfigConnectionStrings(string CS)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (!CS.Contains("="))
            {
                ConnectionStringSettings CSS = ConfigurationManager.ConnectionStrings[CS];
                if (CSS != null)
                    CS = ProcessConnectionStringSettings(CSS);
                else
                    CS = _configuration.GetConnectionString(CS);
            }

            return CS;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private string MapDatabasePath(string ConnectionString)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            ConnectionString = LookupConfigConnectionStrings(ConnectionString);

            if (!ConnectionString.EndsWith(";"))
                ConnectionString += ";";

            ConnectionString = Regex.Replace(ConnectionString, @"DataProvider=(.*?);", "", RegexOptions.IgnoreCase);
            ConnectionString = Util.DecryptTokens(ConnectionString);

            string CurrentPath = "";
#if (!WINDOWS)
            if (_httpContextAccessor != null)
                CurrentPath = _httpContextAccessor.HttpContext.Request.PathBase;
#else
            string AppLocation = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
            CurrentPath = System.IO.Path.GetDirectoryName(AppLocation);
#endif
            string DataSourcePropertyName = "data source";
            switch (Provider)
            {
                case DataProvider.Firebird:
                    DataSourcePropertyName = "database";
                    break;
            }
            ConnectionString = Regex.Replace(ConnectionString, DataSourcePropertyName + "=~", DataSourcePropertyName + "=" + CurrentPath, RegexOptions.IgnoreCase);

            Regex Re = new Regex(DataSourcePropertyName + "=(.*)", RegexOptions.IgnoreCase);
            string[] Parts = ConnectionString.Split(';');

            for (int I = 0; I < Parts.Length; I++)
            {
                if (Re.IsMatch(Parts[I]))
                {
                    string DbPath = Re.Match(Parts[I]).Groups[1].Value;
                    this.DataSourcePath = DbPath;
#if (!WINDOWS)
                    if (_hostingEnvironment != null && DbPath.StartsWith("/") && !DbPath.StartsWith("//"))
                    {
                        var roots = new List<string>() { _hostingEnvironment.ContentRootPath, _hostingEnvironment.WebRootPath };
                        foreach (var root in roots)
                        {
                            var paths = new List<string>() { root };
                            paths.AddRange(DbPath.Split('/'));
                            var filePath = Path.Combine(paths.ToArray());
                            if (File.Exists(filePath))
                            {
                                this.DataSourcePath = filePath;
                                break;
                            }
                        }
                    }
#endif
                    Parts[I] = Parts[I].Replace(DbPath, this.DataSourcePath);
                }
            }

            return String.Join(";", Parts);
        }

        ////////////////////////////////////////////////////////////////////////////
        private string DataSourceFileName()
        ////////////////////////////////////////////////////////////////////////////
        {
            string DbPath = "";
            string DataSourcePropertyName = "data source";
            switch (Provider)
            {
                case DataProvider.Firebird:
                    DataSourcePropertyName = "database";
                    break;
            }

            Regex Re = new Regex(DataSourcePropertyName + "=(/.*)", RegexOptions.IgnoreCase);

            foreach (string Part in ConnectionString.Split(';'))
            {
                if (Re.IsMatch(Part))
                    DbPath = Re.Match(Part).Groups[1].Value;
            }

            return DbPath;
        }

        ////////////////////////////////////////////////////////////////////////////
        private DataTable NpgSqlSchemaInfo(MetaDataType CollectionType)
        ////////////////////////////////////////////////////////////////////////////
        {
            switch (CollectionType)
            {
                case MetaDataType.DataTypes:
                    return NpgsqlDataTypes();
                    break;
                case MetaDataType.Indexes:
                    return GetDataTable("select schemaname as table_schema, tablename as table_name, indexname as index_name  from pg_catalog.pg_indexes");
                    break;
                case MetaDataType.IndexColumns:
                    //				string Sql1 = "SELECT pg_class.relname, pg_attribute.attname, pg_index.* " +
                    //					"FROM	pg_class, pg_attribute, pg_index " +
                    //					"WHERE	pg_class.oid = pg_attribute.attrelid " +
                    //					"AND        pg_class.oid = pg_index.indexrelid " +
                    //					"AND        pg_index.indkey[0] = pg_attribute.attnum " +
                    //					"AND        pg_index.indisprimary = 't'";

                    string Sql = "";

                    for (int I = 0; I < 10; I++)
                    {
                        Sql += "SELECT t.relname as table_name, i.relname as index_name, pg_attribute.attname as column_name,  " + (I + 1).ToString() + " as ORDINAL_POSITION, indisunique as Unique, indisprimary as Primary_Key, indisclustered as Clustered " +
                        "FROM	pg_class t, pg_class i, pg_attribute, pg_index " +
                        "WHERE	t.oid = pg_attribute.attrelid " +
                        "AND    t.oid = pg_index.indrelid " +
                        "AND    i.oid = pg_index.indexrelid " +
                        "AND    pg_index.indkey[" + I.ToString() + "] = pg_attribute.attnum  ";

                        if (I != 9)
                            Sql += " union ";
                    }

                    return GetDataTable(Sql);
                    break;
                default:
                    return new DataTable();
                    break;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        private DataTable SqlServerCESchemaInfo(MetaDataType CollectionType)
        ////////////////////////////////////////////////////////////////////////////
        {
            string SchemaName = "";

            switch (CollectionType)
            {
                case MetaDataType.DataTypes:
                    SchemaName = "PROVIDER_TYPES";
                    break;
                case MetaDataType.Columns:
                    SchemaName = "COLUMNS";
                    break;
                case MetaDataType.Indexes:
                case MetaDataType.IndexColumns:
                    SchemaName = "INDEXES";
                    break;
                case MetaDataType.UserTables:
                case MetaDataType.Tables:
                    SchemaName = "TABLES";
                    break;
                default:
                    return new DataTable();

            }

            DataTable DT = GetDataTable("select * from INFORMATION_SCHEMA." + SchemaName);

            switch (CollectionType)
            {
                case MetaDataType.DataTypes:
                    Hashtable Map = new Hashtable();

                    DT.Columns.Add(new DataColumn("DataType", System.Type.GetType("System.String")));

                    Map["TYPE_NAME"] = "TypeName";
                    Map["DATA_TYPE"] = "ProviderDbType";
                    Map["COLUMN_SIZE"] = "ColumnSize";
                    Map["CREATE_PARAMS"] = "CreateParameters";
                    Map["MINIMUM_SCALE"] = "MinimumScale";
                    Map["MAXIMUM_SCALE"] = "MaximumScale";

                    foreach (string Key in Map.Keys)
                    {
                        try
                        {
                            DT.Columns[Key].ColumnName = Map[Key].ToString();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    Map.Clear();

                    Map["smallint"] = "System.Int16";
                    Map["int"] = "System.Int32";
                    Map["real"] = "System.Single";
                    Map["float"] = "System.Double";
                    Map["money"] = "System.Decimal";
                    Map["bit"] = "System.Boolean";
                    Map["tinyint"] = "System.SByte";
                    Map["bigint"] = "System.Int64";
                    Map["uniqueidentifier"] = "System.Guid";
                    Map["varbinary"] = "System.Byte[]";
                    Map["binary"] = "System.Byte[]";
                    Map["image"] = "System.Byte[]";
                    Map["nvarchar"] = "System.String";
                    Map["nchar"] = "System.String";
                    Map["ntext"] = "System.String";
                    Map["numeric"] = "System.Decimal";
                    Map["datetime"] = "System.DateTime";

                    foreach (DataRow R in DT.Rows)
                    {
                        try
                        {
                            R["DataType"] = Map[R["TypeName"].ToString()].ToString();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;
            }

            return DT;

        }

        ////////////////////////////////////////////////////////////////////////////
        private DataTable NpgsqlDataTypes()
        ////////////////////////////////////////////////////////////////////////////
        {
            DataTable T = new DataTable();

            T.Columns.Add(new DataColumn("TypeName", System.Type.GetType("System.String")));
            T.Columns.Add(new DataColumn("ProviderDbType", System.Type.GetType("System.String")));
            T.Columns.Add(new DataColumn("ColumnSize", System.Type.GetType("System.Int32")));
            T.Columns.Add(new DataColumn("CreateParameters", System.Type.GetType("System.String")));
            T.Columns.Add(new DataColumn("DataType", System.Type.GetType("System.String")));

            DataRow Row = T.NewRow();

            AddNpgsqlDataType(T, "bytea", "bytea", 255, "", "System.Byte[]");
            AddNpgsqlDataType(T, "boolean", "bool", 1, "", "System.Boolean");
            AddNpgsqlDataType(T, "char", "char", 255, "max. length", "System.String");
            AddNpgsqlDataType(T, "date", "date", 10, "timestamp", "System.DateTime");
            AddNpgsqlDataType(T, "real", "float4", 7, "", "System.Single");
            AddNpgsqlDataType(T, "double precision", "float8", 15, "", "System.Double");
            AddNpgsqlDataType(T, "smallint", "int2", 5, "", "System.Int16");
            AddNpgsqlDataType(T, "int", "int4", 10, "", "System.Int32");
            AddNpgsqlDataType(T, "bigint", "int8", 19, "", "System.Int64");
            AddNpgsqlDataType(T, "decimal", "numeric", 28, "precision, scale", "System.Decimal");
            AddNpgsqlDataType(T, "text", "text", System.Int32.MaxValue, "", "System.String");
            AddNpgsqlDataType(T, "time", "time", 8, "", "System.TimeSpan");
            AddNpgsqlDataType(T, "timestamp", "timestamp", 19, "", "System.DateTime");
            AddNpgsqlDataType(T, "varchar", "varchar", 255, "max. length", "System.String");

            return T;
        }

        ////////////////////////////////////////////////////////////////////////////
        private void AddNpgsqlDataType(DataTable T, string TypeName, string ProviderDbType, int ColumnSize, string CreateParameters, string DataType)
        ////////////////////////////////////////////////////////////////////////////
        {
            DataRow Row = T.NewRow();
            Row["TypeName"] = TypeName;
            Row["ProviderDbType"] = ProviderDbType;
            Row["ColumnSize"] = ColumnSize;
            Row["CreateParameters"] = CreateParameters;
            Row["DataType"] = DataType;
            T.Rows.Add(Row);
        }

        ////////////////////////////////////////////////////////////////////////////
        private void UpdateInsertsTable(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            string TableName = Sql;

            if (Sql.ToLower().IndexOf("insert ") != 0)
                Sql = BuildInsertStatement(Sql, Params);

            if (Adapter.InsertCommand != null)
                if (Adapter.InsertCommand.CommandText != Sql)
                    this.InsertsTable = null;

            if (this.InsertsTable == null)
            {
                Adapter.SelectCommand = Conn.CreateCommand();
                Adapter.SelectCommand.Transaction = Command.Transaction;
                Adapter.SelectCommand.CommandText = BuildSelectStatement(TableName, Params);
                this._BatchInsertSelectSql = Adapter.SelectCommand.CommandText;
                DataSet DS = new DataSet();
                Adapter.Fill(DS);
                this.InsertsTable = DS.Tables[0];

                Adapter.InsertCommand = Conn.CreateCommand();
                Adapter.InsertCommand.CommandText = Sql;
                Adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;

                DataTable ST = this.GetSchemaTable(Adapter.SelectCommand.CommandText);

                Hashtable ColumnNames = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

                foreach (string Key in Params.Keys)
                    ColumnNames.Add(Key, Key);

                switch (this.Provider)
                {
                    case DataProvider.SqlClient:
                        SqlDataAdapter A = (SqlDataAdapter)Adapter;

                        foreach (DataRow R in ST.Rows)
                        {
                            string CN = R["ColumnName"].ToString();
                            if (!ColumnNames.Contains(CN))
                                continue;

                            SqlDbType T = (SqlDbType)GetProviderDbType(R["DataTypeName"].ToString());
                            int Size = (int)R["ColumnSize"];
                            A.InsertCommand.Parameters.Add(this.ParameterName(CN), T, Size, CN);
                        }
                        break;

                    case DataProvider.MySql:
                    case DataProvider.OracleClient:
                    case DataProvider.DB2:
                        foreach (DataRow R in ST.Rows)
                        {
                            string CN = R["ColumnName"].ToString();
                            if (!ColumnNames.Contains(CN))
                                continue;

                            object MT = GetProviderDbType(R["DataTypeName"].ToString());
                            int Size = (int)R["ColumnSize"];
                            Object[] Args = new Object[4];
                            Args[0] = this.ParameterName(CN);
                            Args[1] = MT;
                            Args[2] = Size;
                            Args[3] = CN;
                            InvokeMethod(Adapter.InsertCommand.Parameters, "Add", Args);
                        }
                        break;
                }

                this.SetPropertyValue(Adapter, "UpdateBatchSize", this.UpdateBatchSize);
            }

            DataRow Row = this.InsertsTable.NewRow();

            foreach (string Key in Params.Keys)
            {
                if (Params[Key] == null)
                    Row[Key] = System.DBNull.Value;
                else if (Params[Key].ToString() == "" && this.ConvertEmptyToNull)
                    Row[Key] = System.DBNull.Value;
                else
                    Row[Key] = Params[Key];
            }

            this.InsertsTable.Rows.Add(Row);

            if (this.InsertsTable.Rows.Count == this.UpdateBatchSize)
                this.ApplyBatchUpdate();
        }


        ////////////////////////////////////////////////////////////////////////////
        private int GetDatabaseVersion()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (this._Vn != System.Int32.MinValue)
                return this._Vn;

            object Vn = GetPropertyValue(Conn, "ServerVersion");

            Vn = Vn.ToString().Split(' ')[Vn.ToString().Split(' ').Length - 1];

            if (Vn != null)
                this._Vn = Convert.ToInt32(Vn.ToString().Split('.')[0]);
            else
                this._Vn = -1;

            return this._Vn;
        }

        ////////////////////////////////////////////////////////////////////////////
        private string CleanParameterName(string Key)
        ////////////////////////////////////////////////////////////////////////////
        {
            Key = Regex.Replace(Key, "[> ]", "_");

            switch (this.Database)
            {
                case DatabaseType.Oracle:
                    if (IsReservedWord(Key))
                        Key += "_X";
                    break;
            }

            return Key;
        }


        ////////////////////////////////////////////////////////////////////////////
        private void CloseReader()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (_Reader is IDataReader)
                if (!_Reader.IsClosed)
                {
                    try
                    {
                        Command.Cancel();
                    }
                    catch (Exception) { }

                    _Reader.Close();
                }
        }

        ////////////////////////////////////////////////////////////////////////////
        private void AddCommandParameters(IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (Params == null)
                return;

            IDbDataParameter DbParam;

            foreach (string Key in Params.Keys)
            {
                if (Params[Key] is IDbDataParameter)
                {
                    DbParam = (IDbDataParameter)Params[Key];
                    if (DbParam.ParameterName == "")
                        DbParam.ParameterName = CleanParameterName(Key);
                }
                else
                {
                    DbParam = Command.CreateParameter();

                    switch (Provider)
                    {
                        case DataProvider.DB2:
                        case DataProvider.Firebird:
                        case DataProvider.SqlClient:
                        case DataProvider.SqlServerCE:
                            DbParam.ParameterName = ParameterName(Key);
                            break;
                        default:
                            DbParam.ParameterName = CleanParameterName(Key);
                            break;
                    }

                    if (Params[Key] == null)
                        DbParam.Value = System.DBNull.Value;
                    else if (Params[Key].ToString() == "" && this.ConvertEmptyToNull)
                        DbParam.Value = System.DBNull.Value;
                    else
                    {
                        DbParam.Value = Params[Key];

                        if (DbParam is OdbcParameter)
                            if (DbParam.Value is Byte[])
                            {
                                (DbParam as OdbcParameter).DbType = DbType.Binary;
                                (DbParam as OdbcParameter).OdbcType = OdbcType.Image;
                            }

                    }
                }

                Command.Parameters.Add(DbParam);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private DataProvider DeriveProvider(string CS)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            CS = LookupConfigConnectionStrings(CS);

            if (!CS.EndsWith(";"))
                CS += ";";

            if (Regex.IsMatch(CS, "Provider=.*OLEDB.*;", RegexOptions.IgnoreCase))
                return DataProvider.OleDb;

            if (Regex.IsMatch(CS, "Provider=SQLNCLI11.*;", RegexOptions.IgnoreCase))
                return DataProvider.OleDb;

            if (Regex.IsMatch(CS, "^dsn=.*", RegexOptions.IgnoreCase))
                return DataProvider.Odbc;

            if (Regex.IsMatch(CS, @"Data Source=(.*)\.vdb[34];", RegexOptions.IgnoreCase))
                return DataProvider.VistaDB;

            if (Regex.IsMatch(CS, @"Data Source=(.*)\.fdb;", RegexOptions.IgnoreCase))
                return DataProvider.Firebird;

            if (Regex.IsMatch(CS, @"Data Source=(.*)\.sdf;", RegexOptions.IgnoreCase))
                return DataProvider.SqlServerCE;

            if (Regex.IsMatch(CS, @"Data Source=(.*)\.db;", RegexOptions.IgnoreCase))
                return DataProvider.SQLite;

            return ExtractDataProvider(CS);
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static DataProvider ExtractDataProvider(string CS)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (!CS.EndsWith(";"))
                CS += ";";

            if (!Regex.IsMatch(CS, @"DataProvider=(.*?);", RegexOptions.IgnoreCase))
                return DataProvider.SqlClient;

            Match M = Regex.Match(CS, @"DataProvider=(.*?);", RegexOptions.IgnoreCase);

            foreach (DataProvider P in Enum.GetValues(typeof(DataProvider)))
                if (P.ToString().ToLower() == M.Groups[1].Value.ToLower())
                    return P;

            return DataProvider.SqlClient;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static string ProcessConnectionStringSettings(ConnectionStringSettings CSS)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string CS = CSS.ConnectionString;

            if (CSS.ProviderName != "")
                CS += "DataProvider=" + CSS.ProviderName + ";";

            return CS;
        }

        ////////////////////////////////////////////////////////////////////////////
        private object ExecuteScalar(string Sql, IDictionary Params)
        ////////////////////////////////////////////////////////////////////////////
        {
            ConfigureCommand(Sql, Params);
            object RetVal = null;

            try
            {
                RetVal = Command.ExecuteScalar();
            }
            catch (Exception Ex)
            {
                HandleError(Ex);
            }

            WriteTrace();
            return RetVal;
        }

        ////////////////////////////////////////////////////////////////////////////
        private long GetAutoIncrementValue()
        ////////////////////////////////////////////////////////////////////////////
        {
            long Id = -1;

            string Sql = "";
            switch (this.Database)
            {
                case DatabaseType.Access:
                case DatabaseType.Sybase:
                case DatabaseType.MySql:
                case DatabaseType.Pervasive:
                case DatabaseType.VistaDB:
                case DatabaseType.SqlServerCE:
                    Sql = "SELECT @@IDENTITY";
                    break;
                case DatabaseType.DB2:
                    Sql = "SELECT IDENTITY_VAL_LOCAL() FROM SYSIBM.SYSDUMMY1";
                    break;
                case DatabaseType.PostgreSql:
                    Sql = GetPostgreSqlSequence();
                    break;
            }

            if (Sql != "")
            {
                if (ExecuteSingletonQuery(Sql))
                {
                    try
                    {
                        Id = Int64.Parse(_Reader.GetValue(0).ToString());
                    }
                    catch (Exception)
                    {
                    }
                }
                _Reader.Close();
            }

            return Id;
        }


        ////////////////////////////////////////////////////////////////////////////
        private string GetPostgreSqlSequence()
        ////////////////////////////////////////////////////////////////////////////
        {
            Regex RE = new Regex(@"insert\s*into\s*([a-zA-Z0-9_.]*)[\s]*", RegexOptions.IgnoreCase);
            Match M = RE.Match(Command.CommandText);

            if (M.Groups.Count != 2)
                return "";

            string TableName = M.Groups[1].ToString();
            string SchemaName = "";

            if (TableName.Split('.').Length == 2)
            {
                SchemaName = TableName.Split('.')[0];
                TableName = TableName.Split('.')[1];
            }

            ListDictionary Params = new ListDictionary();

            Params.Add("table_name", TableName);

            string Sql = "select column_default " +
                    "from information_schema.columns " +
                    "where column_default like 'nextval(%' " +
                    "and table_name = " + ParameterName("table_name") + " ";

            if (SchemaName != "")
            {
                Sql += "and schema_name = " + ParameterName("schema_name") + " ";
                Params.Add("schema_name", SchemaName);
            }

            if (ExecuteSingletonQuery(Sql, Params))
                return "select " + _Reader.GetValue(0).ToString().Replace("nextval", "currval");
            else
                return "";
        }

        ////////////////////////////////////////////////////////////////////////////
        private Hashtable GetReservedWords()
        ////////////////////////////////////////////////////////////////////////////
        {
            if (_ReservedWords.Count > 0)
                return _ReservedWords;

            DataTable Words = MetaDataCollection(MetaDataType.ReservedWords);

            foreach (DataRow Row in Words.Rows)
                if (Row[0] != null)
                    _ReservedWords[Row[0].ToString().ToUpper()] = true;

            return _ReservedWords;
        }

        ////////////////////////////////////////////////////////////////////////////
        private void HandleError(Exception Ex)
        ////////////////////////////////////////////////////////////////////////////
        {
            System.Diagnostics.StackTrace T = new System.Diagnostics.StackTrace(1);
            System.Diagnostics.StackFrame F = T.GetFrame(0);
            string MethodName = F.GetMethod().DeclaringType.FullName + "." + F.GetMethod().Name;

            string Msg = Ex.Message + System.Environment.NewLine + System.Environment.NewLine;
            string ExMsg = Msg;


            if (Ex.InnerException != null)
            {
                string S = Ex.InnerException.Message + System.Environment.NewLine + System.Environment.NewLine;
                Msg += S;
                ExMsg += "(" + S + ")";
            }

            Msg += "--> Method: " + MethodName + System.Environment.NewLine;
            if (ProviderAssembly != null)
                Msg += "--> Provider: " + ProviderAssembly.FullName + System.Environment.NewLine;

            if (VerboseErrorInfo)
            {
                Msg += CommandErrorInfo();
            }
            else
            {
                if (Conn != null)
                    Msg += "For more information set the VerboseErrorInfo property";
            }


            if (SummaryExceptionMessage)
                throw new Exception(ExMsg);
            else
                throw new Exception(ExMsg, new Exception(Msg));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string CommandErrorInfo()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string Msg = "";

            if (!VerboseErrorInfo)
                return "";

            if (Conn == null)
            {
                if (this.ShowConnectionStringOnError)
                    Msg += "--> Connection: " + this.ConnectionString + System.Environment.NewLine;
            }
            else
            {
                if (CloseConnectionOnError)
                    Conn.Close();

                if (this.ShowConnectionStringOnError)
                    Msg += "--> Connection: " + Conn.ConnectionString + System.Environment.NewLine;

                Msg += "--> Command: " + Command.CommandText + System.Environment.NewLine;
                Msg += "--> Type: " + Command.CommandType.ToString() + System.Environment.NewLine;

                if (Command.Parameters.Count > 0)
                    Msg += "--> Parameters: " + ParameterList() + System.Environment.NewLine;
            }

            return Msg;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal Object InvokeMethod(object Obj, string MethodName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            return InvokeMethod(Obj, MethodName, new Object[0]);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal Object InvokeMethod(object Obj, string MethodName, Object[] Args)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Type[] TypeArray = new Type[Args.Length];

            for (int I = 0; I < Args.Length; I++)
                TypeArray.SetValue(Args[I].GetType(), I);

            MethodInfo MI = Obj.GetType().GetMethod(MethodName, TypeArray);
            Object Result = null;

            if (MI == null)
                throw new Exception("Method --> [" + MethodName + "] not supported by data provider --> " + Obj.GetType().ToString());

            try
            {
                Result = MI.Invoke(Obj, Args);
            }
            catch (Exception)
            {
                return null;
                // HandleError(new Exception(Ex.Message + "==> Invoke:[" + Obj.GetType().ToString() + "." + MethodName + "]"));
            }

            return Result;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal Object GetPropertyValue(object Obj, string PropertyName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Object Value = null;
            PropertyInfo P = Obj.GetType().GetProperty(PropertyName);

            if (P != null)
                if (P.CanRead)
                    try
                    {
                        Value = P.GetValue(Obj, null);
                    }
                    catch (Exception Ex)
                    {
                        HandleError(new Exception(Ex.Message + "==> GetValue:[" + Obj.GetType().ToString() + "." + PropertyName + "]"));
                    }


            return Value;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private SqlDbType GetSqlDbType(string ProviderTypeName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            ProviderTypeName = ProviderTypeName.Split('.')[ProviderTypeName.Split('.').Length - 1];

            switch (ProviderTypeName)
            {
                case "SqlBinary":
                    return SqlDbType.Binary;
                case "SqlBoolean":
                    return SqlDbType.Bit;
                case "SqlByte":
                    return SqlDbType.TinyInt;
                case "SqlDateTime":
                    return SqlDbType.DateTime;
                case "SqlDecimal":
                    return SqlDbType.Decimal;
                case "SqlDouble":
                    return SqlDbType.Float;
                case "SqlFileStream":
                    return SqlDbType.VarBinary;
                case "SqlGuid":
                    return SqlDbType.UniqueIdentifier;
                case "SqlInt16":
                    return SqlDbType.SmallInt;
                case "SqlInt32":
                    return SqlDbType.Int;
                case "SqlInt64":
                    return SqlDbType.BigInt;
                case "SqlMoney":
                    return SqlDbType.Money;
                case "SqlSingle":
                    return SqlDbType.Real;
                case "SqlString":
                    return SqlDbType.VarChar;
                case "SqlXml":
                    return SqlDbType.Xml;
            }

            return SqlDbType.VarChar;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private object GetProviderDbType(string DataTypeName)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Type t = null;

            switch (this.Provider)
            {
                case DataProvider.MySql:
                    t = ProviderAssembly.GetType("MySql.Data.MySqlClient.MySqlDbType");
                    break;
                case DataProvider.OracleClient:
                    t = ProviderAssembly.GetType("System.Data.OracleClient.OracleType");
                    //t = typeof(OracleType);
                    break;
                case DataProvider.SqlClient:
                    t = typeof(SqlDbType);
                    break;
                case DataProvider.DB2:
                    t = ProviderAssembly.GetType("IBM.Data.DB2.DB2Type");
                    break;
            }

            DataTypeName = DataTypeName.Replace("VARCHAR2", "varchar");

            Array values = Enum.GetValues(t);
            string[] names = Enum.GetNames(t);

            for (int I = 0; I < names.Length; I++)
                if (DataTypeName.ToUpper() == names[I].ToUpper())
                    return values.GetValue(I);

            return values.GetValue(0);
        }

        ///////////////////////////////////////////////
        internal object JsonValue(int i)
        ///////////////////////////////////////////////
        {
            if (_Reader.IsDBNull(i))
                return "";

            if (_Reader.GetValue(i) is Byte[])
                return "";

            if (_Reader.GetValue(i) is DateTime)
            {
                DateTime d1 = new DateTime(1970, 1, 1);
                DateTime d2 = Convert.ToDateTime(_Reader.GetValue(i)).ToUniversalTime();
                TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);

                return "/Date(" + Convert.ToInt64(ts.TotalMilliseconds).ToString() + ")/";
            }

            return _Reader.GetValue(i);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void SetPropertyValue(object Obj, string PropertyName, object PropertyValue)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            PropertyInfo P = Obj.GetType().GetProperty(PropertyName);

            if (P != null)
                if (P.CanWrite)
                    try
                    {
                        P.SetValue(Obj, PropertyValue, null);
                    }
                    catch (Exception Ex)
                    {
                        HandleError(new Exception(Ex.Message + "==> SetValue:[" + Obj.GetType().ToString() + "." + PropertyName + "]"));
                    }
        }

        ////////////////////////////////////////////////////////////////////////////
        private string ParameterList()
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Params = new ArrayList();

            foreach (IDbDataParameter P in Command.Parameters)
                Params.Add(P.ParameterName + "=" + ((P.Value == null) ? "NULL" : P.Value.ToString()));
            return string.Join(",", (string[])Params.ToArray(typeof(string)));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void RemapDataTypesSchemaColumnNames(DataTable Schema)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            switch (this.Database)
            {
                case DatabaseType.Oracle:
                case DatabaseType.Pervasive:
                case DatabaseType.MySql:
                    foreach (DataRow Row in Schema.Rows)
                    {
                        if (Row["CreateParameters"].ToString() == "size")
                            Row["CreateParameters"] = "length";
                    }
                    break;
            }

            switch (Provider)
            {
                case DataProvider.DB2:
                    Hashtable Map = new Hashtable();

                    Map["SQL_TYPE_NAME"] = "TypeName";
                    Map["PROVIDER_TYPE"] = "ProviderDbType";
                    Map["COLUMN_SIZE"] = "ColumnSize";
                    Map["CREATE_PARAMS"] = "CreateParameters";
                    Map["FRAMEWORK_TYPE"] = "DataType";
                    Map["AUTO_UNIQUE_VALUE"] = "IsAutoIncrementable";
                    Map["CASE_SENSITIVE"] = "IsCaseSensitive";
                    Map["NULLABLE"] = "IsNullable";
                    Map["SEARCHABLE"] = "IsSearchable";
                    Map["MINIMUM_SCALE"] = "MaximumScale";
                    Map["MAXIMUM_SCALE"] = "MinimumScale";
                    Map["SQL_TYPE"] = "NativeDataType";

                    foreach (string Key in Map.Keys)
                    {
                        try
                        {
                            Schema.Columns[Key].ColumnName = Map[Key].ToString();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void RemapTablesSchemaColumnNames(DataTable Schema)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Hashtable ColumnRemappings = new Hashtable();

            ColumnRemappings["TABLE_SCHEMA"] = new string[] { "OWNER", "SCHEMA", "TABLE_SCHEM", "TABLE_OWNER" };
            ColumnRemappings["TABLE_TYPE"] = new string[] { "TABLETYPE", "TYPE" };
            ColumnRemappings["TABLE_NAME"] = new string[] { "NAME", "VIEW_NAME" };

            foreach (string Key in ColumnRemappings.Keys)
                foreach (string ColumnName in ((string[])ColumnRemappings[Key]))
                    if (Schema.Columns.Contains(ColumnName))
                    {
                        Schema.Columns[ColumnName].ColumnName = Key;
                        break;
                    }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool StartsWithNumeric(string Token)
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            return Regex.IsMatch(Token, @"^\d");
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void CheckSQLCEVersion()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            if (!UpgradeSQLServerCE)
                return;

            Version Version = ProviderAssembly.GetName().Version;
            decimal DLLVersion = Convert.ToDecimal(Version.Major.ToString() + "." + Version.Minor.ToString());

            if (SQLCEVersion() >= DLLVersion)
                return;

            Object[] Args;

            Type SqlCeEngineType = ProviderAssembly.GetType("System.Data.SqlServerCe.SqlCeEngine", true);
            Args = new Object[1];
            Args[0] = this.ConnectionString;

            object SqlCeEngine = Activator.CreateInstance(SqlCeEngineType, Args);

            InvokeMethod(SqlCeEngine, "Upgrade");
            InvokeMethod(SqlCeEngine, "Dispose");
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private decimal SQLCEVersion()
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            Dictionary<int, decimal> versionDictionary = new Dictionary<int, decimal>();

            versionDictionary[0x73616261] = 2m;
            versionDictionary[0x002dd714] = 3m;
            versionDictionary[0x00357b9d] = 3.5m;
            versionDictionary[0x003d0900] = 4m;

            int version = 0;
            try
            {
                using (FileStream fs = new FileStream(this.DataSourcePath, FileMode.Open))
                {
                    fs.Seek(16, SeekOrigin.Begin);
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        version = reader.ReadInt32();
                    }
                }
            }
            catch
            {
                throw;
            }
            if (versionDictionary.ContainsKey(version))
            {
                return versionDictionary[version];
            }
            else
            {
                return -1;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        private void WriteTrace()
        ////////////////////////////////////////////////////////////////////////////
        {
        }

        #endregion
    }
    #endregion


    ////////////////////////////////////////////////////////////////////////////
    internal class Util
    ////////////////////////////////////////////////////////////////////////////
    {
        public static bool EncryptionEnabled = true;
        public static Regex IsEncrypted = new Regex(@"___([a-z0-9A-Z+\/=_]*)___", RegexOptions.Compiled);
        private static string HashKey = "nsdtr";

        ////////////////////////////////////////////////////////////////////////////
        public static string Encrypt(string Str)
        ////////////////////////////////////////////////////////////////////////////
        {
            if (IsEncrypted.IsMatch(Str) || !EncryptionEnabled)
                return Str;

            return XmlConvert.EncodeName("___" + EncDec.Encrypt(Str, HashKey) + "___");
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string Encrypt(string[] Str)
        ////////////////////////////////////////////////////////////////////////////
        {
            ArrayList Tokens = new ArrayList();

            foreach (string S in Str)
                Tokens.Add(Encrypt(S));

            return "[\"" + string.Join("\",\"", (string[])Tokens.ToArray(typeof(string))) + "\"]";
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string Decrypt(string strBase64Text)
        ////////////////////////////////////////////////////////////////////////////
        {
            string S = strBase64Text;
            if (!IsEncrypted.IsMatch(strBase64Text))
                return strBase64Text;

            strBase64Text = XmlConvert.DecodeName(IsEncrypted.Match(strBase64Text).Groups[1].Value);

            return EncDec.Decrypt(strBase64Text, HashKey);
        }

        ////////////////////////////////////////////////////////////////////////////
        public static string DecryptTokens(string S)
        ////////////////////////////////////////////////////////////////////////////
        {
            Match Token = null;

            try
            {
                foreach (Match T in IsEncrypted.Matches(S))
                {
                    Token = T;
                    S = S.Replace(T.Value, Util.Decrypt(T.Value));
                }
            }
            catch (Exception)
            {
            }
            return S;
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    internal class EncDec
    ////////////////////////////////////////////////////////////////////////////
    {
        ////////////////////////////////////////////////////////////////////////////
        internal static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        ////////////////////////////////////////////////////////////////////////////
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);

            cs.Close();

            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        ////////////////////////////////////////////////////////////////////////////
        internal static string Encrypt(string clearText, string Password)
        ////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
                return Convert.ToBase64String(encryptedData);
            }
            catch (Exception)
            {
                return clearText;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        internal static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        ////////////////////////////////////////////////////////////////////////////
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        ////////////////////////////////////////////////////////////////////////////
        internal static string Decrypt(string cipherText, string Password)
        ////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
                return System.Text.Encoding.Unicode.GetString(decryptedData);
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
