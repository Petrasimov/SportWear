
using System.ComponentModel.DataAnnotations;

namespace SportWear.Api.DTOs
{
    public class ProductCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue)]
        public int BrandId { get; set; }

        public List<ProductSizeCreateDto> Sizes { get; set; } = new();
    }
}
