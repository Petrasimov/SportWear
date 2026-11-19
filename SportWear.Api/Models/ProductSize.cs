namespace SportWear.Api.Models
{
    public class ProductSize
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int StockQuantity { get; set; }
        public Product Product { get; set; } = null!;
        public Size Size { get; set; } = null!;
    }
}
