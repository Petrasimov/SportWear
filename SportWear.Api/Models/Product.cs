using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SportWear.Api.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Precision(18,2)]
        public decimal Price { get; set; }
        public string? Description { get; set; } 
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public Category Category { get; set; } = null!;
        public Brand Brand { get; set; } = null!;
        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();

    }
}
