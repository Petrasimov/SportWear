
namespace SportWear.Api.DTOs
{
    public class ProductSizeDto
    {
        public int Id { get; set; }

        public int SizeId { get; set; }

        public string SizeName { get; set; } = string.Empty;

        public int StockQuantity { get; set; }
    }
}
