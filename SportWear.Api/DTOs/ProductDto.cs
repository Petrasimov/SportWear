
namespace SportWear.Api.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int BrandId { get; set; }

        public string BrandName { get; set; } = string.Empty;

        public List<ProductSizeDto> Sizes { get; set; } = new();
    }
}
