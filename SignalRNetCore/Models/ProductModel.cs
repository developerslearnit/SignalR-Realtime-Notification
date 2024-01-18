namespace SignalRNetCore.Models
{
    public class ProductModel
    {
        public string ProductName { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
