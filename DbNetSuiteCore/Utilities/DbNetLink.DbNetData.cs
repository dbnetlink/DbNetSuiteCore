using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data.OleDb;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;

namespace DbNetLink.Old.Data


{
    #region Enumerators
    public enum DatabaseType
    {
        Access,
        Access2007,
        Advantage,
        dBASE,
        DB2,
        Excel,
        Excel2007,
        Firebird,
        InterSystemsCache,
        MySql,
        Oracle,
        Paradox,
        Pervasive,
        PostgreSql,
        Progress,
        SQLite,
        SqlServer,
        SqlServerCE,
        Sybase,
        TextFile,
        VistaDB,
        VisualFoxPro,
        Unknown
    };

    public enum MetaDataType
    {
        MetaDataCollections,
        Columns,
        Databases,
        DataSourceInformation,
        DataTypes,
        ForeignKeys,
        Functions,
        IndexColumns,
        Indexes,
        PrimaryKeys,
        Procedures,
        ProcedureParameters,
        Restrictions,
        ReservedWords,
        Tables,
        Views,
        ViewColumns,
        UserDefinedTypes,
        Users,
        UserTables,
        UserViews
    };

    #endregion

    #region CommandConfig
    [Serializable]
    public class CommandConfig
    {
        public string Sql = String.Empty;
        public ListDictionary Params = new ListDictionary();

        public CommandConfig()
            : this("")
        {
        }

        public CommandConfig(string Sql)
        {
            this.Sql = Sql;
        }
    }
    #endregion

    #region QueryCommandConfig
    [Serializable]
    public class QueryCommandConfig : CommandConfig
    {
        public CommandBehavior Behavior = CommandBehavior.Default;

        public QueryCommandConfig()
            : this("")
        {
        }
        public QueryCommandConfig(string Sql)
            : base(Sql)
        {
        }
    }

    #endregion

    #region UpdateCommandConfig

    public class UpdateCommandConfig : CommandConfig
    {
        public ListDictionary FilterParams = new ListDictionary();

        public UpdateCommandConfig()
            : this("")
        {
        }
        public UpdateCommandConfig(string Sql)
            : base(Sql)
        {
        }
    }
    #endregion


    #region DbNetData
    public class DbNetData : IDisposable
    {
        #region Delegates
        public delegate void CommandConfiguredHandler(DbNetData ConnectonInstance, IDbCommand Command);
        #endregion

        #region Events
        public event CommandConfiguredHandler OnCommandConfigured = null;
        #endregion

        #region Public Properties
        public IDbDataAdapter Adapter;
        public IDbConnection Conn = null;
        public IDbCommand Command;
        public IDataReader Reader;
        public IDbTransaction Transaction;
        public bool CloseConnectionOnError = true;
        public bool InjectionDetectionEnabled = false;
        public int CommandTimeout = -1;
        public string ConnectionString
        {
            get { return _ConnectionString; }
        }
        public DatabaseType Database
        {
            get { return _Database; }
        }
        public DataProvider Provider
        {
            get { return _Provider; }
        }
        public long Identity = -1;
        public int CommandDurationWarningThreshold = 100;
        public string NameDelimiterTemplate = "{0}";
        public bool ReturnAutoIncrementValue = false;
        public long RowsAffected;
        public string DataSourcePath = "";
        public bool VerboseErrorInfo = true;
        public bool AllowUnqualifiedUpdates = false;
        public bool SummaryExceptionMessage = false;
        public bool ConvertEmptyToNull = true;
        public bool ShowConnectionStringOnError = false;
        public bool QualifyAllObjectNames = false;
        public bool UpgradeSQLServerCE = false;
        public int UpdateBatchSize
        {
            get { return _UpdateBatchSize; }
            set { if (this.IsBatchUpdateSupported) _UpdateBatchSize = value; }
        }
        public Hashtable ReservedWords
        {
            get { return GetReservedWords(); }
        }
        public int DatabaseVersion
        {
            get { return GetDatabaseVersion(); }
        }
        public bool IsBatchUpdateSupported
        {
            get { return _BatchUpdateSupported; }
        }
        #endregion

        #region Private Properties

        private Hashtable _ReservedWords = new Hashtable();

        private DateTime CommandStart;
        private DataRow DataSourceInfo = null;
        internal Assembly ProviderAssembly;
        internal string ParameterTemplate = "@{0}";
        private DataTable InsertsTable = null;

        private DatabaseType _Database = DatabaseType.Unknown;
        private DataProvider _Provider = DataProvider.SqlClient;
        private IWebHostEnvironment _Env = null;
        private string _ConnectionString = "";
        private int _Vn = System.Int32.MinValue;
        private bool _BatchUpdateSupported = false;
        private int _UpdateBatchSize = 1;
        private string _BatchInsertSelectSql = "";
        private List<string> InjectionStrings = new List<string> { "--", ";", "xp_", "/*","*/" };

        #endregion

        #region Constructors


       
        public DbNetData(string connectionString, IWebHostEnvironment env, IConfiguration configuration )
            : this(configuration.GetConnectionString(connectionString), DeriveProvider(configuration.GetConnectionString(connectionString)), env)
        {

        }

        public DbNetData(string connectionString, IWebHostEnvironment env)
            : this(connectionString, DeriveProvider(connectionString), env)
        {

        }

        public DbNetData(string ConnectionString, DataProvider Provider, IWebHostEnvironment env, DatabaseType? Database = null)
        {
            this._Env = env;
            this._Provider = Provider;
            this._ConnectionString = MapDatabasePath(ConnectionString);
            CustomDataProvider customDataProvider = null;

            try
            {
                switch (Provider)
                {
                    case DataProvider.OleDb:
                        Conn = new OleDbConnection(this.ConnectionString);
                        Adapter = new OleDbDataAdapter();
                        ProviderAssembly = Assembly.GetAssembly(typeof(OleDbConnection));
                        break;
                    case DataProvider.SQLite:
                        Conn = new SqliteConnection(this.ConnectionString);
                        ProviderAssembly = Assembly.GetAssembly(typeof(SqliteConnection));
                        this._Database = DatabaseType.SQLite;
                        break;
                    case DataProvider.SqlClient:
                        Conn = new SqlConnection(this.ConnectionString);
                        Adapter = new SqlDataAdapter();
                        this._Database = DatabaseType.SqlServer;
                        ProviderAssembly = Assembly.GetAssembly(typeof(SqlConnection));
                        break;
                    case DataProvider.MySql:
                        customDataProvider = new CustomDataProvider("MySql.Data", "MySqlClient.MySqlConnection");
                        this._Database = DatabaseType.MySql;
                        break;
                    default:
                        break;

                }
            }
            catch (Exception Ex)
            {
                HandleError(Ex);
            }

            if (customDataProvider != null)
            {
                ProviderAssembly = Assembly.Load(customDataProvider.AssemblyName);
                Type connectionType = ProviderAssembly.GetType(customDataProvider.ConnectionTypeName, true);
                Type adapterType = ProviderAssembly.GetType(customDataProvider.AdapterTypeName, true);

                Object[] args = new Object[1];
                args[0] = this.ConnectionString;

                Conn = (IDbConnection)Activator.CreateInstance(connectionType, args);
                Adapter = (IDbDataAdapter)Activator.CreateInstance(adapterType);
            }

            Command = Conn.CreateCommand();

            if (Adapter != null)
            {
                Adapter.SelectCommand = Command;
            }

        }
        #endregion

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DbNetData()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        public void AddColumn(string TableName, string ColumnName, Type DataType, object DefaultValue)
        {
            AddColumn(TableName, ColumnName, DataType, 50, DefaultValue);
        }

        public void AddColumn(string TableName, string ColumnName, Type DataType)
        {
            AddColumn(TableName, ColumnName, DataType, 50, null);
        }

        public void AddColumn(string TableName, string ColumnName, Type DataType, int Length)
        {
            AddColumn(TableName, ColumnName, DataType, Length, null);
        }

        public void AddColumn(string TableName, string ColumnName, Type DataType, int Length, object DefaultValue)
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

        public void AddColumn(string TableName, string ColumnName, string ColumnDef)
        {
            AddColumn(TableName, ColumnName, ColumnDef, null);
        }

        public void AddColumn(string TableName, string ColumnName, string ColumnDef, object DefaultValue)
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
                    ColumnDef = Regex.Replace(ColumnDef, "^text&", "clob", RegexOptions.IgnoreCase);
                    ColumnDef = Regex.Replace(ColumnDef, "^datetime", "date", RegexOptions.IgnoreCase);
                    break;
                case DatabaseType.Access:
                case DatabaseType.Access2007:
                    ColumnDef = Regex.Replace(ColumnDef, "^text$", "memo", RegexOptions.IgnoreCase);
                    ColumnDef = Regex.Replace(ColumnDef, "bit", "yesno", RegexOptions.IgnoreCase);
                    ColumnDef = Regex.Replace(ColumnDef, "int", "short", RegexOptions.IgnoreCase);
                    ColumnDef = Regex.Replace(ColumnDef, "^varchar", "text", RegexOptions.IgnoreCase);
                    break;
                case DatabaseType.DB2:
                    ColumnDef = Regex.Replace(ColumnDef, "^text$", "varchar(8000)", RegexOptions.IgnoreCase);
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
        public bool ColumnExists(string TableName, string ColumnName)
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

        public void AddTable(string TableName)
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
                this.ExecuteNonQuery(EncodeIllegalChar(SqlStmnt));
        }

        public void AddIndex(string TableName, string[] Columns)
        {
            string SqlStmnt = "create index " + TableName + "_idx on " + TableName + "(" + String.Join(",", Columns) + ")";
            this.ExecuteNonQuery(SqlStmnt, true);
        }


        public string GetColumnDataType(string TableName, string ColumnName)
        {
            string Sql = "select " + ColumnName + " from " + TableName + " where 1=2";
            DataTable DT = this.GetSchemaTable(Sql);
            return ((Type)DT.Rows[0]["DataType"]).Name;
        }

        public bool TableExists(string TableName)
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
        public void AddViewColumn(string ViewName, string ColumnExpression)
        {
            if (this.Database == DatabaseType.SqlServerCE)
                return;

            QueryCommandConfig Query = new QueryCommandConfig();
            Query.Sql = "select view_definition from information_schema.views where table_name = @view_name";
            Query.Params["view_name"] = ViewName;

            if (!this.ExecuteSingletonQuery(Query))
                return;

            String ViewDefinition = this.ReaderValue("view_definition").ToString().Replace(System.Environment.NewLine, " ");
            Match M = Regex.Match(ViewDefinition, @"select (.*)(from .*)", RegexOptions.IgnoreCase);

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


        public void ApplyBatchUpdate()
        {
            if (this.InsertsTable == null)
                return;

            switch (this.Provider)
            {
                case DataProvider.SqlClient:
                    ((SqlDataAdapter)Adapter).Update(this.InsertsTable);
                    break;
                default:
                    throw new Exception("Batch update not supported by this data provider");
            }

            this.InsertsTable.Rows.Clear();
        }

        public bool ColumnIsNull(string ColumnName)
        {
            try
            {
                return Reader.IsDBNull(Reader.GetOrdinal(ColumnName));
            }
            catch (Exception)
            {
                throw new Exception("ReaderValue column not found: " + ColumnName);
            }
        }


        public bool ColumnInReader(string ColumnName)
        {
            for (int i = 0; i < Reader.FieldCount; i++)
                if (Reader.GetName(i).Equals(ColumnName, StringComparison.InvariantCultureIgnoreCase)) return true;

            return false;
        }

        public string UpdateConcatenationOperator(string expr)
        {
            switch (Database)
            {
                case DatabaseType.SQLite:
                    expr = expr.Replace("+", "||");
                    break;
            }

            return expr;
        }

        public void BeginTransaction()
        {
            Transaction = Conn.BeginTransaction();
            Command.Transaction = Transaction;
        }

        public void Rollback()
        {
            Transaction.Rollback();
        }

        public void Commit()
        {
            try
            {

                Transaction.Commit();
            }
            catch (Exception)
            {
            }
        }

        public void Close()
        {
            CloseReader();

            if (Conn.State == ConnectionState.Open)
            {
                Conn.Close();
            }
        }

    

        public ListDictionary ParseParameters(string Sql)
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

        public ListDictionary DeriveParameters(string ProcedureName)
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


        public long ExecuteDelete(string Sql)
        {
            return ExecuteDelete(Sql, new ListDictionary());
        }

        public long ExecuteDelete(CommandConfig CmdConfig)
        {
            return ExecuteDelete(CmdConfig.Sql, CmdConfig.Params);
        }

        public long ExecuteDelete(string Sql, IDictionary Params)
        {
            if (Sql.ToLower().IndexOf("delete ") != 0)
                Sql = BuildDeleteStatement(Sql, Params);

            this.RowsAffected = ExecuteNonQuery(Sql, Params);
            return this.RowsAffected;
        }

        public long ExecuteInsert(string Sql)
        {
            return ExecuteInsert(Sql, new ListDictionary());
        }


        public long ExecuteInsert(CommandConfig CmdConfig)
        {
            return ExecuteInsert(CmdConfig.Sql, CmdConfig.Params);
        }

        public long ExecuteInsert(string Sql, IDictionary Params)
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
                Sql += EncodeIllegalChar(";select scope_identity();");
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


        public int ExecuteNonQuery(CommandConfig CmdConfig)
        {
            return ExecuteNonQuery(CmdConfig.Sql, CmdConfig.Params, false);
        }

        public int ExecuteNonQuery(string Sql)
        {
            return ExecuteNonQuery(Sql, new ListDictionary(), false);
        }

        public int ExecuteNonQuery(string Sql, IDictionary Params)
        {
            return ExecuteNonQuery(Sql, Params, false);
        }

        public int ExecuteNonQuery(string Sql, bool IgnoreErrors)
        {
            return ExecuteNonQuery(Sql, new ListDictionary(), IgnoreErrors);
        }


        public int ExecuteNonQuery(string Sql, IDictionary Params, bool IgnoreErrors)
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

            return RetVal;
        }

        public object ExecuteScalar(CommandConfig Config)
        {
            return ExecuteScalar(Config.Sql, Config.Params);
        }

        public void RunScript(string Sql)
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

        public void ExecuteQuery(CommandConfig CmdConfig)
        {
            ExecuteQuery(CmdConfig.Sql, CmdConfig.Params);
        }

        public void ExecuteQuery(QueryCommandConfig CmdConfig)
        {
            ExecuteQuery(CmdConfig.Sql, CmdConfig.Params, CmdConfig.Behavior);
        }

        public void ExecuteQuery(string Sql)
        {
            ExecuteQuery(Sql, new ListDictionary(), CommandBehavior.Default);
        }

        public void ExecuteQuery(string Sql, IDictionary Params)
        {
            ExecuteQuery(Sql, Params, CommandBehavior.Default);
        }

        public void ExecuteQuery(string Sql, IDictionary Params, CommandBehavior Behaviour)
        {
            if (Sql.ToLower().IndexOf("select ") == 0)
                if (Sql.ToLower().IndexOf(" where ") == -1 && Params.Count > 0)
                    Sql = AddWhereClause(Sql, Params);

            ConfigureCommand(Sql, Params);

            try
            {
                Reader = Command.ExecuteReader(Behaviour);
            }
            catch (Exception Ex)
            {
                HandleError(Ex);
            }

        }

        public bool ExecuteSingletonQuery(string Sql)
        {
            return ExecuteSingletonQuery(Sql, new ListDictionary(), CommandBehavior.Default);
        }

        public bool ExecuteSingletonQuery(QueryCommandConfig Query)
        {
            return ExecuteSingletonQuery(Query.Sql, Query.Params, Query.Behavior);
        }

        public bool ExecuteSingletonQuery(string Sql, IDictionary Params)
        {
            return ExecuteSingletonQuery(Sql, Params, CommandBehavior.Default);
        }


        public bool ExecuteSingletonQuery(string Sql, IDictionary Params, CommandBehavior Behavior)
        {
            ExecuteQuery(Sql, Params, Behavior);
            return Reader.Read();
        }

        public long ExecuteUpdate(UpdateCommandConfig CmdConfig)
        {
            return ExecuteUpdate(CmdConfig.Sql, CmdConfig.Params, CmdConfig.FilterParams);
        }


        public long ExecuteUpdate(string Sql)
        {
            return ExecuteUpdate(Sql, new ListDictionary(), new ListDictionary());
        }

        public long ExecuteUpdate(string Sql, IDictionary Params)
        {
            return ExecuteUpdate(Sql, Params, new ListDictionary());
        }

        public long ExecuteUpdate(string Sql, IDictionary Params, IDictionary FilterParams)
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

        public DataSet GetDataSet(string Sql)
        {
            return GetDataSet(Sql, new ListDictionary());
        }

        public DataSet GetDataSet(QueryCommandConfig CmdConfig)
        {
            return GetDataSet(CmdConfig.Sql, CmdConfig.Params);
        }

        public DataSet GetDataSet(string Sql, IDictionary Params)
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
            return DS;
        }

        public DataTable GetDataTable(string Sql)
        {
            return GetDataTable(Sql, new ListDictionary());
        }

        public DataTable GetDataTable(QueryCommandConfig CmdConfig)
        {
            DataTable dataTable = new DataTable();
            ExecuteQuery(CmdConfig);
            dataTable.Load(Reader);
            return dataTable;
        }


        public DataTable GetDataTable(string Sql, IDictionary Params)
        {
            return GetDataSet(Sql, Params).Tables[0];
        }


       
        public DataTable GetSchemaTable(string Sql)
        {
            return GetSchemaTable(Sql, new ListDictionary());
        }

        public DataTable GetSchemaTable(string Sql, ListDictionary Params)
        {
            string[] TableName = new string[0];
            if (Sql.ToLower().IndexOf("select ") != 0)
            {
                TableName = Sql.Split('.');
                Sql = "select * from " + this.QualifiedDbObjectName(Sql) + " where 1=2";
            }

            ExecuteQuery(Sql, Params, CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
            DataTable dataTable = Reader.GetSchemaTable();

            if (dataTable == null)
                return null;

            if (!dataTable.Columns.Contains("DataTypeName"))
                dataTable.Columns.Add(new DataColumn("DataTypeName", System.Type.GetType("System.String")));

            dataTable.Columns.Add(new DataColumn("FieldTypeName", System.Type.GetType("System.String")));
            dataTable.Columns.Add(new DataColumn("ProviderFieldTypeName", System.Type.GetType("System.String")));
            dataTable.Columns.Add(new DataColumn("DefaultValue", System.Type.GetType("System.String")));

            dataTable.Columns["DataTypeName"].ReadOnly = false;

            for (int i = 0; i < Reader.FieldCount; i++)
            {
                var dataTypeName = Reader.GetDataTypeName(i);
                var fieldType = Reader.GetFieldType(i);
                dataTable.Rows[i]["DataTypeName"] = dataTypeName ?? string.Empty;
                dataTable.Rows[i]["FieldTypeName"] = fieldType == null ? string.Empty : fieldType.ToString();

                switch (Reader.GetType().Name)
                {
                    case "SqlDataReader":
                        Object[] Args = new Object[1];
                        Args[0] = i;
                        var providerFieldTypeName = InvokeMethod(Reader, "GetProviderSpecificFieldType", Args);
                        dataTable.Rows[i]["ProviderFieldTypeName"] = providerFieldTypeName == null ? string.Empty : providerFieldTypeName.ToString();
                        break;
                }

            }

            Reader.Close();

            if (this.Database == DatabaseType.SqlServer && TableName.Length > 0)
            {
                Sql = "SELECT K.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE K ON T.CONSTRAINT_NAME = K.CONSTRAINT_NAME WHERE T.CONSTRAINT_TYPE = 'PRIMARY KEY' AND T.TABLE_NAME = '" + TableName[TableName.Length - 1] + "'";

                if (TableName.Length == 2)
                {
                    Sql += " AND T.TABLE_SCHEMA = '" + TableName[0] + "'";
                }

                ExecuteQuery(Sql);
                while (Reader.Read())
                {
                    if (dataTable.Columns["IsKey"].ReadOnly)
                    {
                        dataTable.Columns["IsKey"].ReadOnly = false;
                        foreach (DataRow R in dataTable.Rows)
                            R["IsKey"] = false;
                    }
                    DataRow[] Rows = dataTable.Select("ColumnName = '" + Reader.GetString(0) + "'");
                    Rows[0]["IsKey"] = true;
                }

                Sql = "SELECT COLUMN_NAME, COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + TableName[TableName.Length - 1] + "'";

                if (TableName.Length == 2)
                {
                    Sql += " AND TABLE_SCHEMA = '" + TableName[0] + "'";
                }


                DataTable DefaultValues = GetDataTable(Sql);

                foreach (DataRow row in DefaultValues.Rows)
                {
                    DataRow[] Rows = dataTable.Select("ColumnName = '" + row["COLUMN_NAME"] + "'");

                    if (Rows.Length > 0)
                    {
                        Rows[0]["DefaultValue"] = row["COLUMN_DEFAULT"];
                    }
                }
            }

            if (this.Database == DatabaseType.SqlServer)
            {
                Sql = "SELECT COLUMN_NAME, COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + dataTable.Rows[0]["BaseTableName"] + "'";

                if (dataTable.Rows[0]["BaseSchemaName"] != null && dataTable.Rows[0]["BaseSchemaName"].ToString() != String.Empty)
                {
                    Sql += " AND TABLE_SCHEMA = '" + dataTable.Rows[0]["BaseSchemaName"] + "'";
                }

                DataTable DefaultValues = GetDataTable(Sql);

                foreach (DataRow row in DefaultValues.Rows)
                {
                    DataRow[] Rows = dataTable.Select("ColumnName = '" + row["COLUMN_NAME"] + "'");

                    if (Rows.Length > 0)
                    {
                        Rows[0]["DefaultValue"] = row["COLUMN_DEFAULT"];
                    }
                }
            }

            switch (this.Database)
            {
                case DatabaseType.MySql:
                    dataTable.Columns["ColumnSize"].ReadOnly = false;

                    foreach (DataRow Row in dataTable.Rows)
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
                    dataTable.Columns["ColumnSize"].ReadOnly = false;

                    foreach (DataRow Row in dataTable.Rows)
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
                    dataTable.Columns["NumericPrecision"].ReadOnly = false;
                    dataTable.Columns["NumericScale"].ReadOnly = false;

                    foreach (DataRow Row in dataTable.Rows)
                    {
                        if (Row["NumericPrecision"] == System.DBNull.Value)
                            Row["NumericPrecision"] = 0;
                        if (Row["NumericScale"] == System.DBNull.Value)
                            Row["NumericScale"] = 0;
                    }
                    break;
                case DatabaseType.PostgreSql:
                    dataTable.Columns["ColumnSize"].ReadOnly = false;

                    foreach (DataRow Row in dataTable.Rows)
                    {
                        if (Row["ColumnSize"].ToString() == "-1")
                            Row["ColumnSize"] = System.Int32.MaxValue;
                    }
                    break;
            }

            return dataTable;
        }

        public Int64 GetSequenceValue(string SequenceName, bool Increment)
        {

            switch (this.Database)
            {
                case DatabaseType.Oracle:
                    break;
                default:
                    throw new Exception("GetSequenceValue not supported for this database");
            }

            string Sql = "select " + SequenceName + "." + ((Increment) ? "next" : "curr") + "val from dual";
            this.ExecuteSingletonQuery(Sql);
            return Convert.ToInt64(Reader.GetValue(0));

        }

        public bool IsReservedWord(string Token)
        {
            return (ReservedWords[Token.ToUpper()] != null);
        }


        public ListDictionary JsonRecord()
        {
            ListDictionary Data = new ListDictionary();

            if (Reader != null)
                if (!Reader.IsClosed)
                    for (int i = 0; i < Reader.FieldCount; i++)
                        Data[Reader.GetName(i).ToLower()] = JsonValue(i);

            return Data;
        }


        public DataTable MetaDataCollection(MetaDataType CollectionType)
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

        public DataTable MetaDataCollection(string CollectionType)
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

        public void Open()
        {
            if (this.Provider == DataProvider.SqlServerCE)
                CheckSQLCEVersion();

            if (Conn.State != ConnectionState.Open)
            {
                try
                {
                    Conn.Open();
                }
                catch (Exception Ex)
                {
                    HandleError(Ex);
                }
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
                case DatabaseType.SQLite:
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

        public string ParameterName(string Key)
        {
            if (Key.Length > 0 && ParameterTemplate.Length > 1)
                if (ParameterTemplate.Substring(0, 1) == Key.Substring(0, 1))
                    return Key;

            return ParameterTemplate.Replace("{0}", CleanParameterName(Key));
        }

        public string QualifiedDbObjectName(string ObjectName, char Delimiter)
        {
            string[] ObjectNames = ObjectName.Split(Delimiter);
            for (int i = 0; i < ObjectNames.Length; i++)
                ObjectNames[i] = QualifiedDbObjectName(ObjectNames[i]);

            return String.Join(Delimiter.ToString(), ObjectNames);
        }

        public string QualifiedDbObjectName(string ObjectName, bool split = true)
        {
            string[] NameParts = split ? ObjectName.Split('.') : new string[] {ObjectName};

            Regex RE = new Regex(NameDelimiterTemplate.Replace("[", @"\[").Replace("]", @"\]").Replace("{0}", ".*"));

            for (int I = 0; I < NameParts.Length; I++)
                if (Regex.IsMatch(NameParts[I], @"\W") || IsReservedWord(NameParts[I]) || StartsWithNumeric(NameParts[I]) || this.QualifyAllObjectNames)
                    if (!RE.IsMatch(NameParts[I]))
                        NameParts[I] = NameDelimiterTemplate.Replace("{0}", NameParts[I].Trim());

            return string.Join(".", NameParts);
        }

        public string UnqualifiedDbObjectName(string ObjectName)
        {
            return UnqualifiedDbObjectName(ObjectName, false);
        }

        public string UnqualifiedDbObjectName(string ObjectName, bool BaseNameOnly)
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


        public string ReaderString(string ColumnName)
        {
            return ReaderValue(ColumnName).ToString();
        }


        public string ReaderString(int ColumnIndex)
        {
            return ReaderValue(ColumnIndex).ToString();
        }
        public object ReaderValue(string ColumnName)
        {
            try
            {
                return ReaderValue(Reader.GetOrdinal(ColumnName));
            }
            catch (Exception)
            {
                throw new Exception("ReaderValue column not found: " + ColumnName);
            }

        }

        public object ReaderValue(int ColumnIndex)
        {
            return Reader.GetValue(ColumnIndex);
        }

        public void SetParamValue(ListDictionary Params, string ParamName, object ParamValue)
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


        public string UserTableFilter()
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

        public string UserViewFilter()
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
        private string AddWhereClause(string Sql, IDictionary Params)
        {
            if (Params.Count == 0)
                return Sql;

            return Sql + " where " + BuildParamFilter(Params);
        }

        internal string BuildParamFilter(IDictionary Params)
        {
            if (Params.Count == 0)
                return "";

            List<string> Parameters = new List<string>();

            foreach (string Key in Params.Keys)
                Parameters.Add(QualifiedDbObjectName(Key) + " = " + ParameterName(Key));

            return "(" + string.Join(" and ", Parameters.ToArray()) + ")";
        }


        private string BuildUpdateStatement(string TableName, IDictionary Params, IDictionary FilterParams)
        {
            List<string> Parameters = new List<string>();

            foreach (string Key in Params.Keys)
                Parameters.Add(QualifiedDbObjectName(Key) + " = " + ParameterName(Key));

            string Sql = "update " + TableName + " set " + string.Join(",", Parameters.ToArray());

            return this.AddWhereClause(Sql, FilterParams);
        }

        private string BuildInsertStatement(string TableName, IDictionary Params)
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

        private string BuildSelectStatement(string TableName, IDictionary Params)
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

        private string BuildDeleteStatement(string TableName, IDictionary Params)
        {
            return this.AddWhereClause("delete from " + TableName, Params);
        }

        private string EncodeIllegalChar(string Sql)
        {
            if (!InjectionDetectionEnabled)
                return Sql;

            return Regex.Replace(Sql, ";", EncodedChar(";"));
        }

        private string DecodeIllegalChar(string Sql)
        {
            if (!InjectionDetectionEnabled)
                return Sql;

            return Regex.Replace(Sql, EncodedChar(";"), ";");
        }

        private string EncodedChar(string Char)
        {
#if (!WINDOWS)
            return "##" + HttpUtility.UrlEncode(Char).Replace("%", String.Empty) + "##";
#else
                return Char;
#endif
        }

        internal CommandType GetCommandType(string commandText)
        {
            if (Regex.Match(commandText, "^(alter|drop|create|select|insert|delete|update|set|if|begin|print|open) ", RegexOptions.IgnoreCase).Success)
                return CommandType.Text;
            else
                return CommandType.StoredProcedure;
        }

        private void ConfigureCommand(string Sql, IDictionary Params)
        {
            CloseReader();
            Command.CommandText = Sql.Trim();

            if (InjectionDetectionEnabled)
            {
                foreach ( string injectionString in InjectionStrings )
                {
                    if (Command.CommandText.IndexOf(injectionString, StringComparison.OrdinalIgnoreCase) >=0 )
                        throw new Exception("Illegal characters detected in SQL [" + Command.CommandText + "]");
                }

                Command.CommandText = DecodeIllegalChar(Command.CommandText);
            }

            Command.CommandType = GetCommandType(Command.CommandText);

            if (this.CommandTimeout > -1)
                if (this.Database != DatabaseType.SqlServerCE)
                    Command.CommandTimeout = this.CommandTimeout;

            Command.Parameters.Clear();
            AddCommandParameters(Params);

            if (OnCommandConfigured != null)
                OnCommandConfigured(this, Command);

            CommandStart = System.DateTime.Now;
        }


        private DatabaseType GetDatabaseType()
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
                return GetDatabaseTypeFromFileName();
            if (DataSourceProductName.IndexOf("db2") > -1)
                return DatabaseType.DB2;
            if (DataSourceProductName.IndexOf("firebird") > -1)
                return DatabaseType.Firebird;
            if (DataSourceProductName.IndexOf("sybase") > -1 || DataSourceProductName.IndexOf("ase server") > -1)
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

        private DatabaseType GetDatabaseTypeFromFileName()
        {
            string FileName = DataSourceFileName();
            string Ext = Path.GetExtension(FileName);

            switch (Ext.ToLower())
            {
                case ".xls":
                case ".csv":
                    return DatabaseType.Excel;
                case ".xlsx":
                    return DatabaseType.Excel2007;
                case ".mdb":
                    return DatabaseType.Access;
                case ".accdb":
                    return DatabaseType.Access2007;
                default:
                    return DatabaseType.Access;
            }
        }


        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }

            }

            return dt;
        }
     
     
        private string MapDatabasePath(string ConnectionString)
        {
            if (!ConnectionString.EndsWith(";"))
                ConnectionString += ";";

            string DataDirectory = String.Empty;

            if (AppDomain.CurrentDomain.GetData("DataDirectory") != null)
                DataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();

            if (ConnectionString.Contains("|DataDirectory|") && DataDirectory != String.Empty)
                ConnectionString = ConnectionString.Replace("|DataDirectory|", DataDirectory);

            ConnectionString = Regex.Replace(ConnectionString, @"DataProvider=(.*?);", "", RegexOptions.IgnoreCase);
            ConnectionString = Util.DecryptTokens(ConnectionString);

            string CurrentPath = "";

            if (this._Env != null)
                CurrentPath = this._Env.WebRootPath;

            string DataSourcePropertyName = "data source";

            ConnectionString = Regex.Replace(ConnectionString, DataSourcePropertyName + "=~", DataSourcePropertyName + "=" + CurrentPath, RegexOptions.IgnoreCase).Replace("=//", "=/");
            return ConnectionString;
        }

        public string DataSourceFileName()
        {
            string DbPath = "";
            string DataSourcePropertyName = "data source";
            switch (Provider)
            {
                case DataProvider.Firebird:
                    DataSourcePropertyName = "database";
                    break;
            }

            Regex Re = new Regex(DataSourcePropertyName + "=(.*)", RegexOptions.IgnoreCase);

            foreach (string Part in ConnectionString.Split(';'))
            {
                if (Re.IsMatch(Part))
                    DbPath = Re.Match(Part).Groups[1].Value;
            }

            return DbPath;
        }

        private DataTable NpgSqlSchemaInfo(MetaDataType CollectionType)
        {
            switch (CollectionType)
            {
                case MetaDataType.DataTypes:
                    return NpgsqlDataTypes();
                case MetaDataType.Indexes:
                    return GetDataTable("select schemaname as table_schema, tablename as table_name, indexname as index_name  from pg_catalog.pg_indexes");
                case MetaDataType.IndexColumns:
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
                default:
                    return new DataTable();
            }
        }

        private DataTable SqlServerCESchemaInfo(MetaDataType CollectionType)
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

        private DataTable NpgsqlDataTypes()
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

        private void AddNpgsqlDataType(DataTable T, string TypeName, string ProviderDbType, int ColumnSize, string CreateParameters, string DataType)
        {
            DataRow Row = T.NewRow();
            Row["TypeName"] = TypeName;
            Row["ProviderDbType"] = ProviderDbType;
            Row["ColumnSize"] = ColumnSize;
            Row["CreateParameters"] = CreateParameters;
            Row["DataType"] = DataType;
            T.Rows.Add(Row);
        }

        private void UpdateInsertsTable(string Sql, IDictionary Params)
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

                switch (this.Provider)
                {
                    case DataProvider.SqlClient:
                        SqlDataAdapter A = (SqlDataAdapter)Adapter;

                        foreach (DataRow R in ST.Rows)
                        {
                            SqlDbType T = (SqlDbType)GetProviderDbType(R["DataTypeName"].ToString());
                            int Size = (int)R["ColumnSize"];
                            string CN = R["ColumnName"].ToString();
                            A.InsertCommand.Parameters.Add(this.ParameterName(CN), T, Size, CN);
                        }
                        break;

                    case DataProvider.MySql:
                    case DataProvider.OracleClient:
                    case DataProvider.DB2:
                        foreach (DataRow R in ST.Rows)
                        {
                            object MT = GetProviderDbType(R["DataTypeName"].ToString());
                            int Size = (int)R["ColumnSize"];
                            string CN = R["ColumnName"].ToString();
                            Object[] Args = new Object[4];
                            Args[0] = this.ParameterName(CN);
                            Args[1] = MT;
                            Args[2] = Size;
                            Args[3] = CN;
                            InvokeMethod(Adapter.InsertCommand.Parameters, "Add", Args);
                        }
                        break;
                }

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


        private int GetDatabaseVersion()
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

        public string CleanParameterName(string Key)
        {
            Key = Regex.Replace(Key, "[^a-zA-Z0-9_]", "_");

            switch (this.Database)
            {
                case DatabaseType.Oracle:
                    if (IsReservedWord(Key))
                        Key += "_X";
                    break;
            }

            return Key;
        }


        private void CloseReader()
        {
            if (Reader is IDataReader)
                if (!Reader.IsClosed)
                {
                    try
                    {
                        if (Provider != DataProvider.Odbc)
                            Command.Cancel();
                    }
                    catch (Exception) { }

                    Reader.Close();
                }
        }

        private void AddCommandParameters(IDictionary Params)
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
                        case DataProvider.SQLite:
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

        public static DataProvider DeriveProvider(string connectionString)
        {
            if (!connectionString.EndsWith(";"))
                connectionString += ";";

            if (Regex.IsMatch(connectionString, "Provider=.*OLEDB.*;", RegexOptions.IgnoreCase))
                return DataProvider.OleDb;

            if (Regex.IsMatch(connectionString, "Provider=SQLNCLI11.*;", RegexOptions.IgnoreCase))
                return DataProvider.OleDb;

            if (Regex.IsMatch(connectionString, "^dsn=.*", RegexOptions.IgnoreCase))
                return DataProvider.Odbc;

            if (Regex.IsMatch(connectionString, @"Data Source=(.*)\.vdb[345];", RegexOptions.IgnoreCase))
                return DataProvider.VistaDB;

            if (Regex.IsMatch(connectionString, @"Data Source=(.*)\.fdb;", RegexOptions.IgnoreCase))
                return DataProvider.Firebird;

            if (Regex.IsMatch(connectionString, @"Data Source=(.*)\.sdf;", RegexOptions.IgnoreCase))
                return DataProvider.SqlServerCE;

            if (Regex.IsMatch(connectionString, @"Data Source=(.*)\.db;", RegexOptions.IgnoreCase))
                return DataProvider.SQLite;

            if (Regex.IsMatch(connectionString, "port=3306;", RegexOptions.IgnoreCase))
                return DataProvider.MySql;

            return ExtractDataProvider(connectionString);
        }


        private static DataProvider ExtractDataProvider(string CS)
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


        private object ExecuteScalar(string Sql, IDictionary Params)
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

            return RetVal;
        }

        private long GetAutoIncrementValue()
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
                        Id = Int64.Parse(Reader.GetValue(0).ToString());
                    }
                    catch (Exception)
                    {
                    }
                }
                Reader.Close();
            }

            return Id;
        }


        private string GetPostgreSqlSequence()
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
                return "select " + Reader.GetValue(0).ToString().Replace("nextval", "currval");
            else
                return "";
        }

        private Hashtable GetReservedWords()
        {
            if (_ReservedWords.Count > 0)
                return _ReservedWords;

            DataTable Words = MetaDataCollection(MetaDataType.ReservedWords);

            foreach (DataRow Row in Words.Rows)
                if (Row[0] != null)
                    _ReservedWords[Row[0].ToString().ToUpper()] = true;

            return _ReservedWords;
        }

        private void HandleError(Exception Ex)
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

        internal string CommandErrorInfo()
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
                Msg += "--> Timeout: " + Command.CommandTimeout.ToString() + System.Environment.NewLine;

                if (Command.Parameters.Count > 0)
                    Msg += "--> Parameters: " + ParameterList() + System.Environment.NewLine;
            }

            return Msg;
        }

        internal Object InvokeMethod(object Obj, string MethodName)
        {
            return InvokeMethod(Obj, MethodName, new Object[0]);
        }

        internal Object InvokeMethod(object Obj, string MethodName, Object[] Args)
        {
            string Message;
            return InvokeMethod(Obj, MethodName, Args, out Message);
        }

        internal Object InvokeMethod(object Obj, string MethodName, Object[] Args, out string Message)
        {
            Message = String.Empty;
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
            catch (Exception Ex)
            {
                if (Ex.InnerException != null)
                    Message = Ex.InnerException.Message;
                else
                    Message = Ex.Message;
                return null;
            }

            return Result;
        }

        internal Object GetPropertyValue(object Obj, string PropertyName)
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

        private SqlDbType GetSqlDbType(string ProviderTypeName)
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

        private object GetProviderDbType(string DataTypeName)
        {
            Type t = null;

            switch (this.Provider)
            {
                case DataProvider.MySql:
                    t = ProviderAssembly.GetType("MySql.Data.MySqlClient.MySqlDbType");
                    break;
                case DataProvider.OracleClient:
#if NET40
                    t = ProviderAssembly.GetType("System.Data.OracleClient.OracleType");
#endif
#if NET20
                    t = typeof(OracleType);
#endif
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

        internal object JsonValue(int i)
        {
            if (Reader.IsDBNull(i))
                return "";

            if (Reader.GetValue(i) is Byte[])
                return "";

            if (Reader.GetValue(i) is DateTime)
            {
                DateTime d1 = new DateTime(1970, 1, 1);
                DateTime d2 = Convert.ToDateTime(Reader.GetValue(i)).ToUniversalTime();
                TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);

                return "/Date(" + Convert.ToInt64(ts.TotalMilliseconds).ToString() + ")/";
            }

            return Reader.GetValue(i);
        }

        internal void SetPropertyValue(object Obj, string PropertyName, object PropertyValue)
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

        private string ParameterList()
        {
            ArrayList Params = new ArrayList();

            foreach (IDbDataParameter P in Command.Parameters)
                Params.Add(P.ParameterName + "=" + ((P.Value == null) || P.Value == DBNull.Value ? "NULL" : P.Value.ToString()));
            return string.Join(",", (string[])Params.ToArray(typeof(string)));
        }

        private void RemapDataTypesSchemaColumnNames(DataTable Schema)
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


        private void RemapTablesSchemaColumnNames(DataTable Schema)
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

        private bool StartsWithNumeric(string Token)
        {
            return Regex.IsMatch(Token, @"^\d");
        }

        private void CheckSQLCEVersion()
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

            string Message;

            InvokeMethod(SqlCeEngine, "Upgrade", new object[0], out Message);

            if (Message != String.Empty)
                throw new Exception(Message);

            InvokeMethod(SqlCeEngine, "Dispose");
        }

        private decimal SQLCEVersion()
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

        #endregion
    }
    #endregion


    internal class Util
    {
        public static bool EncryptionEnabled = true;
        public static Regex IsEncrypted = new Regex(@"___([a-z0-9A-Z+\/=_]*)___", RegexOptions.Compiled);
        private static string HashKey = "nsdtr";

        public static string Encrypt(string Str)
        {
            if (IsEncrypted.IsMatch(Str) || !EncryptionEnabled)
                return Str;

            return XmlConvert.EncodeName("___" + EncDec.Encrypt(Str, HashKey) + "___");
        }

        public static string Encrypt(string[] Str)
        {
            ArrayList Tokens = new ArrayList();

            foreach (string S in Str)
                Tokens.Add(Encrypt(S));

            return "[\"" + string.Join("\",\"", (string[])Tokens.ToArray(typeof(string))) + "\"]";
        }

        public static string Decrypt(string strBase64Text)
        {
            string S = strBase64Text;
            if (!IsEncrypted.IsMatch(strBase64Text))
                return strBase64Text;

            strBase64Text = XmlConvert.DecodeName(IsEncrypted.Match(strBase64Text).Groups[1].Value);

            return EncDec.Decrypt(strBase64Text, HashKey);
        }

        public static string DecryptTokens(string S)
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


    internal class EncDec
    {
        internal static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            return clearData;
        }

        internal static string Encrypt(string clearText, string Password)
        {
            try
            {
                return clearText;
            }
            catch (Exception)
            {
                return clearText;
            }
        }

        internal static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            return cipherData;
        }

        internal static string Decrypt(string cipherText, string Password)
        {
            try
            {
                return cipherText;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
