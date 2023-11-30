namespace DbNetSuiteCore.Web.UI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Discontinued { get; set; }
        public DateTime Created { get; set; }
        public decimal UnitPrice { get; set; }
        public int MinimumReorderLevel { get; set; }
    }
}
