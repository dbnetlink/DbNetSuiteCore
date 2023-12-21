using System.Text;
using System.Net.Http.Json;
using DbNetSuiteCore.Helpers;
using AngleSharp.Html.Dom;
using DbNetSuiteCore.Models.DbNetGrid;
using DbNetSuiteCore.Enums;
using Microsoft.Data.Sqlite;
using DbNetSuiteCore.Tests.Models;
using Dapper;
using Newtonsoft.Json.Linq;
using DbNetSuiteCore.Extensions;
using DbNetSuiteCore.Tests.Extensions;
using DbNetSuiteCore.Constants.DbNetGrid;
using System.Data;

namespace DbNetSuiteCore.Tests.DbNetGrid
{
    public class DbNetGridTests : DbNetSuiteTests
    {
        public DbNetGridTests() : base() { }

        protected DbNetGridRequest GetRequest(string fromPart = "customers", string connection = "northwind")
        {
            DbNetGridRequest request = new DbNetGridRequest();
            request.ConnectionString = EncodingHelper.Encode(connection);
            request.FromPart = EncodingHelper.Encode(fromPart);
            request.DataProvider = DataProvider.SQLite;
            return request;
        }
        protected DbNetGridRequest GetRequest<T>(string fromPart = "customers", string connection = "northwind")
        {
            DbNetGridRequest request = new DbNetGridRequest();
            request.ConnectionString = string.Empty;
            request.FromPart = EncodingHelper.Encode(fromPart);
            request.Json = JArray.FromObject(GetList<T>(fromPart, connection));
            request.JsonType = typeof(T);
            request.DataProvider = DataProvider.DataTable;
            return request;
        }

        protected async Task<JArray> GetLookup(string sql, string connection = "northwind")
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("value", typeof(int));
            dataTable.Columns.Add("text", typeof(string));

            using (var conn = new SqliteConnection(GetConnectionString(connection)))
            {
                await conn.OpenAsync();
                IDataReader reader = await conn.ExecuteReaderAsync(sql);

                while (reader.Read())
                {
                    dataTable.Rows.Add(reader.GetValue(0), reader.GetValue(1));
                }

                await conn.CloseAsync();
            }

            JArray jArray = JArray.FromObject(dataTable);
            return jArray;   
        }

        protected async Task<DbNetGridResponse?> GetResponse(DbNetGridRequest request, string action)
        {
            if (request.DataProvider == DataProvider.DataTable && action == RequestAction.Initialize)
            {
                var propertyTypes = request.JsonType.PropertyTypes();
                foreach (string name in propertyTypes.Keys)
                {
                    Type type = propertyTypes[name];
                    type = Nullable.GetUnderlyingType(type) ?? type;
                    request.GetColumn(name).DataType = type.ToString().Split(".").Last();
                }
            }
            request.Columns.Where(c => c.Index < 0).ToList().ForEach(c => c.EncodeClientProperties());

            StringContent json = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync($"/dbnetgrid.dbnetsuite?action={action}", json);
            DbNetGridResponse? dbNetGridResponse = await response.Content.ReadFromJsonAsync<DbNetGridResponse>();
            return dbNetGridResponse;
        }

        protected int CellIndex(IHtmlDocument document, string columnName)
        {
            var thead = document.QuerySelector("thead");
            var headingRow = thead!.Children.First() as IHtmlTableRowElement;
            foreach (IHtmlTableCellElement cell in headingRow!.Cells)
            {
                if (cell.Dataset["columnname"]!.ToLower() == columnName.ToLower())
                {
                    return Convert.ToInt16(cell.Dataset["columnordinal"]) - 1;
                }
            }
            return -1;
        }

        private IEnumerable<T> GetList<T>(string tableName, string connection)
        {
            IEnumerable<T> data = new List<T>();
            using (var conn = new SqliteConnection(GetConnectionString(connection)))
            {
                SqlMapper.AddTypeHandler(new BooleanTypeHandler());
                SqlMapper.AddTypeHandler(new DecimalTypeHandler());
                data = conn.Query<T>($"select * from {tableName};");
            }

            return data;
        }
    }
}