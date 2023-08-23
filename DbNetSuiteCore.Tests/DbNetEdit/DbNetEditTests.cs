using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using DbNetSuiteCore.Helpers;
using Irony.Parsing;
using AngleSharp.Html.Dom;
using DbNetSuiteCore.Models.DbNetEdit;

namespace DbNetSuiteCore.Tests.DbNetEdit
{
    public class DbNetEditTests : DbNetSuiteTests
    {
        public DbNetEditTests() : base() {}

        protected DbNetEditRequest GetRequest(string fromPart = "customers", string connection = "northwind")
        {
            DbNetEditRequest request = new DbNetEditRequest();
            request.ConnectionString = EncodingHelper.Encode(connection);
            request.FromPart = EncodingHelper.Encode(fromPart);
            return request;
        }
        protected async Task<DbNetEditResponse?> GetResponse(DbNetEditRequest request, string action)
        {
            request.Columns.Where(c => c.Index < 0).ToList().ForEach(c => c.EncodeClientProperties());
            StringContent json = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync($"/DbNetEdit.dbnetsuite?action={action}", json);
            DbNetEditResponse? DbNetEditResponse = await response.Content.ReadFromJsonAsync<DbNetEditResponse>();
            return DbNetEditResponse;
        }

    }
}