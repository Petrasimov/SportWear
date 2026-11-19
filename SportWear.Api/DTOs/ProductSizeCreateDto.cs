
using System.ComponentModel.DataAnnotations;

namespace SportWear.Api.DTOs
{
    public class ProductSizeCreateDto
    {
        [Range(1, int.MaxValue)]
        public int SizeId { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}
