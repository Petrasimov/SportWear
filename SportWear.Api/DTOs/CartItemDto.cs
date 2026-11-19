
namespace SportWear.Api.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }

        public int ProductSizeId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string SizeName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public int StockQuantity { get; set; }

        public decimal Subtotal { get; set; }
    }
}
