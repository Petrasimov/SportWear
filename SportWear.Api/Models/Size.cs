using System.ComponentModel.DataAnnotations;

namespace SportWear.Api.Models
{
    public class Size
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Name { get; set; } = string.Empty;
        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
    }
}
