using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using DbNetSuiteCore.Helpers;
using Irony.Parsing;
using AngleSharp.Html.Dom;
using DbNetSuiteCore.Models.DbNetGrid;

namespace DbNetSuiteCore.Tests.DbNetGrid
{
    public class DbNetGridTests : DbNetSuiteTests
    {
        public DbNetGridTests() : base() {}

        protected DbNetGridRequest GetRequest(string fromPart = "customers", string connection = "northwind")
        {
            DbNetGridRequest request = new DbNetGridRequest();
            request.ConnectionString = EncodingHelper.Encode(connection);
            request.FromPart = EncodingHelper.Encode(fromPart);
            return request;
        }
        protected async Task<DbNetGridResponse?> GetResponse(DbNetGridRequest request, string action)
        {
            request.Columns.Where(c => c.Index < 0).ToList().ForEach(c => c.EncodeClientProperties());
            StringContent json = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync($"/dbnetgrid.dbnetsuite?action={action}", json);
            DbNetGridResponse? dbNetGridResponse = await response.Content.ReadFromJsonAsync<DbNetGridResponse>();
            return dbNetGridResponse;
        }

        protected int CellIndex(IHtmlDocument document, string columnName )
        {
            var thead = document.QuerySelector("thead");
            var headingRow = thead!.Children.First() as IHtmlTableRowElement;
            foreach (IHtmlTableCellElement cell in headingRow!.Cells)
            {
                if (cell.Dataset["columnname"]!.ToLower() == columnName.ToLower())
                {
                    return Convert.ToInt16(cell.Dataset["columnordinal"]) -1;
                }
            }
            return -1;
        }
    }
}