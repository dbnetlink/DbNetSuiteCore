using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models.DbNetCombo;

namespace DbNetSuiteCore.Tests.DbNetCombo
{
    public class DbNetComboTests : DbNetSuiteTests
    {
        public DbNetComboTests() : base() {}

        protected DbNetComboRequest GetRequest(string fromPart = "customers",string connection = "northwind")
        {
            DbNetComboRequest request = new DbNetComboRequest();
            request.ConnectionString = EncodingHelper.Encode(connection);
            request.FromPart = EncodingHelper.Encode(fromPart);
            return request;
        }
        protected async Task<DbNetComboResponse?> GetResponse(DbNetComboRequest request, string action)
        {
            StringContent json = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync($"/dbnetcombo.dbnetsuite?action={action}", json);
            DbNetComboResponse? dbNetComboResponse = await response.Content.ReadFromJsonAsync<DbNetComboResponse>();
            return dbNetComboResponse;
        }
    }
}